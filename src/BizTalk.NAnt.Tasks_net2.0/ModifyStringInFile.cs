using System;
using System.IO;
using System.Xml;
using System.Net;

using System.Xml.XPath;
using System.Collections;

using System.Text.RegularExpressions;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.NAnt.Tasks
{
   /// <summary>
   /// This task modifies the file specified by "filespec" by replacing all instances of the search string
   /// with a string constructed with the search string and a format specifier.
   /// i.e. if search string is "foo", and format string is "4.{0}", the output will be "4.foo" and all instances
   /// of foo will be replaced in the file fileSpec.
   /// 
   /// Alternatively, if you supply "groupname", then searchstring is interpreted as a regular expression.
   /// In this case, if:
   /// searchstring = MQS://[\w]*/[\w]*/(?<QueueName>[a-zA-Z]+[\w\.]*)
   /// groupname = QueueName
   /// formatspec = "4.{0}"
   /// Then everything that matches QueueName within searchstring will have "4." prefixed on it.
   /// </summary>
   [TaskName("modifystringinfile")]
   public class ModifyStringInFile : Task
   {
      private string _fileSpec;
      private string _formatSpec;
      private string _searchString;
      private string _groupName = string.Empty;

      public ModifyStringInFile()
      {
         //
         // TODO: Add constructor logic here
         //
      }

      [TaskAttribute("file", Required=true)]
      [StringValidator(AllowEmpty=false)]
      public string FileSpec
      {
         get{return _fileSpec;}
         set
         {
            _fileSpec = value;
         }
      }

      [TaskAttribute("formatspec", Required=true)]
      [StringValidator(AllowEmpty=true)]
      public string FormatSpec
      {
         get{return _formatSpec;}
         set{_formatSpec = value;}
      }

      [TaskAttribute("searchstring", Required=true)]
      [StringValidator(AllowEmpty=false)]
      public string SearchString
      {
         get{return _searchString;}
         set{_searchString = value;}
      }

      // If this is populated, SearchString is interpreted as a regex, and
      // the value of the GroupName is what is replaced.
      [TaskAttribute("groupname", Required=true)]
      [StringValidator(AllowEmpty=true)]
      public string GroupName
      {
         get{return _groupName;}
         set{_groupName = value;}
      }


      protected override void ExecuteTask()
      {
         string contents;
         System.Text.Encoding encoding;
         using(StreamReader sr = File.OpenText(_fileSpec))
         {
            contents = sr.ReadToEnd();
            encoding = sr.CurrentEncoding;
         }

         if(_groupName != string.Empty)
         {
            contents = Regex.Replace(contents,_searchString,new MatchEvaluator(SupplyContent));          
         }
         else
         {
            contents = Regex.Replace(contents,_searchString,string.Format(_formatSpec,_searchString));          
         }

         using(FileStream fs = File.OpenWrite(_fileSpec))
         {
            byte[] content = encoding.GetBytes(contents);
            fs.Write(content,0,content.Length);
         }

      }

      private string SupplyContent(Match match)
      {
         string origContent = match.Groups[0].Captures[0].Value;
         string groupContent = match.Groups[_groupName].Captures[0].Value;
         string newGroupContent = string.Format(_formatSpec,groupContent);
         string newContent = Regex.Replace(origContent,groupContent,newGroupContent); 
        
         return newContent;
      }
   }
}
