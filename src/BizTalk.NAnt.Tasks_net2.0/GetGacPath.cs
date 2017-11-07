// Scott Colestock / traceofthought.net

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;

using System.Xml.XPath;
using System.Collections;
using System.Reflection;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace BizTalk.NAnt.Tasks
{
	/// <summary>
	/// This class returns what will be (or currently is) the full path in the file system to the gac location of a 
	/// component, given its short name and a full path (non-gac) to the assembly.
	/// This can be useful if you want to push a PDB file to that location.
	/// Of course, any change introduced to how the .net framework manages the gac will break this.
	/// </summary>
	[TaskName("getgacpath")]
	public class GetGacPath : Task
	{
		private string _fullPath;
		private string _property;

		// This is appropriate for .net 1.0/1.1
		// For .NET 2.0, you would specify GAC_MSIL (for portable binaries)
		// or GAC_32/GAC_64.
		private string _gacVariant = "gac";

		public GetGacPath()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		[TaskAttribute("gacvariant", Required=false)]
		[StringValidator(AllowEmpty=true)]
		public string GacVariant
		{
			get{return _gacVariant;}
			set
			{
				_gacVariant = value;
			}
		}

		[TaskAttribute("fullpath", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string FullPath
		{
			get{return _fullPath;}
			set
			{
				_fullPath = value;
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
			props[_property] = GetGacPathHelper.GetGacPath.GetPath(
				_fullPath,
				_gacVariant);

		}

	}

}
