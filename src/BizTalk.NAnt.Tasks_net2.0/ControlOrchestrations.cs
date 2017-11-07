// Scott Colestock / traceofthought.net

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Net;
using System.Xml.XPath;
using System.Collections;
using System.Management;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.NAnt.Tasks
{
	/// <summary>
	/// Summary description for ControlOrchestrations
	/// </summary>
	[TaskName("controlorchestrations")]
	public class ControlOrchestrations : Task
	{
		//private string _orchNames;
		private string _orchAssembly;
		private bool _startMode;
		private BtsCatalogExplorer _catalog;
		private string _orchestrationsToIgnore = "";

		public ControlOrchestrations()
		{
			// connect to the local BizTalk configuration database
			_catalog = new BtsCatalogExplorer();
			_catalog.ConnectionString = string.Format("Server={0};Initial Catalog={1};Integrated Security=SSPI;",
			                                          BizTalkGroupInfo.GroupDBServerName,
			                                          BizTalkGroupInfo.GroupMgmtDBName);
		}

		/// <summary>
		/// The mode attribute indicates if we are starting or stopping orchestrations.
		/// </summary>
		[TaskAttribute("mode", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string Mode
		{
			get { return _startMode ? "start" : "stop"; }
			set
			{
				if (value != "stop" && value != "start")
					throw(new BuildException("mode attribute must have value of start or stop.", this.Location));

				_startMode = value == "start" ? true : false;
			}
		}

		[TaskAttribute("orchestrationstoignore", Required=false)]
		[StringValidator(AllowEmpty=true)]
		public string OrchestrationsToIgnore
		{
			get { return _orchestrationsToIgnore; }
			set { _orchestrationsToIgnore = value; }
		}

		/// <summary>
		/// The orchassembly attribute indicates whether we are starting or stopping/unenlisting orchestrations.
		/// If we are stopping, existing instances will be terminatd as well.
		/// </summary>
		[TaskAttribute("orchassembly", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string OrchAssembly
		{
			get { return _orchAssembly; }
			set { _orchAssembly = value; }
		}

		protected override void ExecuteTask()
		{
			PropertyDictionary props = this.Properties;

			this.Log(Level.Info, "\ncontrolorchestrations:\n");

			//get the ignored orchestrations
			StringCollection collOrchestrationsToIgnore = new StringCollection();
			if (_orchestrationsToIgnore != null)
			{
				Regex whiteSpaceAndComma = new Regex(@"\s*,+\s*");
				string[] ignoredOrchestrations = whiteSpaceAndComma.Split(_orchestrationsToIgnore.Trim());
				collOrchestrationsToIgnore.AddRange(ignoredOrchestrations);
			}

			if (_startMode)
				this.Log(Level.Info, "Starting...\n");
			else
			{
				this.Log(Level.Info, "Stopping/Unenlisting/Terminating Existing Instances...\n");

				// Terminate existing instances for this assembly.
				string query = string.Format("SELECT * FROM MSBTS_ServiceInstance WHERE AssemblyName = \"{0}\"", _orchAssembly);
				ManagementObjectSearcher mos = new ManagementObjectSearcher(
					new ManagementScope(@"root\MicrosoftBizTalkServer"),
					new WqlObjectQuery(query), null);
				foreach (ManagementObject asm in mos.Get())
				{
					asm.InvokeMethod("Terminate", new object[] {});
				}
			}

			try
			{
				BtsAssembly btsAssembly = _catalog.Assemblies[_orchAssembly];
				if (btsAssembly == null)
				{
					if (_startMode)
					{
						throw(new BuildException(string.Format("Orchestration assembly '{0}' not found.", _orchAssembly), this.Location));
					}
					else
					{
						this.Log(Level.Warning, "Orchestration assembly '{0}' not found (not attempting Stop/Unenlist)", _orchAssembly);
						_catalog.DiscardChanges();
						return;
					}
				}

				BtsOrchestrationCollection orchestrations = btsAssembly.Orchestrations;

				foreach (BtsOrchestration orchestration in orchestrations)
				{
					if (collOrchestrationsToIgnore.Contains(orchestration.FullName))
					{
						this.Log(Level.Info, "\t***Skipping {0}", orchestration.FullName);
						continue;
					}
					else
					{
						this.Log(Level.Info, "\t{0}", orchestration.FullName);
					}

					if (_startMode)
					{
						this.Log(Level.Info, "\tStarting {0}", orchestration.FullName);
						orchestration.Status = OrchestrationStatus.Started;
					}
					else
					{
						orchestration.AutoTerminateInstances = true;
						orchestration.Status = OrchestrationStatus.Unenlisted;
					}

				}

				this.Log(Level.Info, "(Committing changes to the catalog)...\n");
				_catalog.SaveChanges();
			}
			catch (System.Exception ex)
			{
				this.Log(Level.Info, "(Discarding changes to the catalog)...\n");
				this.Log(Level.Info, ex.ToString());
				_catalog.DiscardChanges();
				throw new BuildException("Failed to control orchestrations.", this.Location, ex);
			}
		}

	}
}
