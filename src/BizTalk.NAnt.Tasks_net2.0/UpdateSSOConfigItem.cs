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
	/// Summary description for AddSSOConfigItem.
	/// </summary>
	[TaskName("updatessoconfigitem")]
	public class UpdateSSOConfigItem : Task
	{
        private string _bizTalkAppName;
		private string _ssoItemName;
		private string _ssoItemValue;

        [TaskAttribute("biztalkappname", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string BizTalkAppName
        {
            get { return _bizTalkAppName; }
            set
            {
                _bizTalkAppName = value;
            }
        }

		[TaskAttribute("ssoitemname", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string SSOItemName
		{
			get{return _ssoItemName;}
			set
			{
				_ssoItemName = value;
			}
		}

		[TaskAttribute("ssoitemvalue", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string SSOItemValue
		{
			get{return _ssoItemValue;}
			set
			{
				_ssoItemValue = value;
			}
		}


		protected override void ExecuteTask()
		{
			PropertyDictionary props = this.Properties;
            string appName = null;

            if (string.IsNullOrEmpty(_bizTalkAppName))
            {
                appName = Project.ProjectName;
            }
            else
            {
                appName = _bizTalkAppName;
            }

			Hashtable ht = SSOSettingsFileManager.SSOSettingsFileReader.Read(appName);
			ht[_ssoItemName] = _ssoItemValue;
			SSOSettingsFileManager.SSOSettingsFileReader.Update(appName,ht);

			this.Log(Level.Info,"Updated SSO config item with name: {0}",_ssoItemName);
		}
	}
}
