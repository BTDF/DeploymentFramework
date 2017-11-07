// Scott Colestock / traceofthought.net

using System;
using System.IO;
using System.Xml;
using System.Net;

using System.Xml.XPath;
using System.Collections;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.NAnt.Tasks
{
   /// <summary>
   /// This task returns the directory name corresponding to a full filespec, i.e. c:\inetpub\wwwroot if
   /// the filespec property is set to c:\inetpub\wwwroot\myfile.htm
   /// </summary>
   [TaskName("getdirectoryname")]
   public class GetDirectoryName : Task
   {
      private string _fileSpec;
      private string _property;

      public GetDirectoryName()
		{
			//
			// TODO: Add constructor logic here
			//
		}

      [TaskAttribute("filespec", Required=true)]
      [StringValidator(AllowEmpty=false)]
      public string FileSpec
      {
         get{return _fileSpec;}
         set
         {
            _fileSpec = value;
         }
      }

      [TaskAttribute("property", Required=true)]
      [StringValidator(AllowEmpty=false)]
      public string Property
      {
         get{return _property;}
         set{_property = value;}
      }


      protected override void ExecuteTask()
      {
         PropertyDictionary props = this.Properties;
         props[_property] = System.IO.Path.GetDirectoryName(_fileSpec);
      }

	}
}
