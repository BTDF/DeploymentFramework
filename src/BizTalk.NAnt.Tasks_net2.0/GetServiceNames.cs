// Scott Colestock / traceofthought.net

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Net;
using System.Management;
using System.Text;

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
	/// This task returns the names of Windows services that match a particular pattern
	/// as a comma delimited string into the specified nant property.
	/// </summary>
	[TaskName("getservicenames")]
	public class GetServiceNames : Task
	{
		private string _serviceMatch;
		private string _property;

		public GetServiceNames()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		[TaskAttribute("servicematch", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string ServiceMatch
		{
			get{return _serviceMatch;}
			set
			{
				_serviceMatch = value;
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

			// Terminate existing instances for this assembly.
			string query = string.Format("SELECT * FROM Win32_Service WHERE DisplayName LIKE \"{0}\"", _serviceMatch);
			Trace.WriteLine("query: " + query);
			ManagementObjectSearcher mos = new ManagementObjectSearcher(
				new ManagementScope(@"root\cimv2"),
				new WqlObjectQuery(query), null);
			ArrayList ar = new ArrayList();
			foreach (ManagementObject mo in mos.Get())
			{
				object o = mo.GetPropertyValue("DisplayName");
				ar.Add(o.ToString());
			}

			string[] svcs = (string[])ar.ToArray(typeof(string));
         
			props[_property] = string.Join(",",svcs);
		}

	}
}
