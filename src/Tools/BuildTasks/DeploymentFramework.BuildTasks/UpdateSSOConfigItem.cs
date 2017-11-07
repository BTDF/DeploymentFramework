// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DeploymentFramework.BuildTasks
{
	public class UpdateSSOConfigItem : Task
	{
        private string _bizTalkAppName;
		private string _ssoItemName;
		private string _ssoItemValue;

        [Required]
        public string BizTalkAppName
        {
            get { return _bizTalkAppName; }
            set
            {
                _bizTalkAppName = value;
            }
        }

		[Required]
		public string SSOItemName
		{
			get{return _ssoItemName;}
			set
			{
				_ssoItemName = value;
			}
		}

		[Required]
		public string SSOItemValue
		{
			get{return _ssoItemValue;}
			set
			{
				_ssoItemValue = value;
			}
		}

        public override bool Execute()
        {
            Hashtable ht = SSOSettingsFileManager.SSOSettingsFileReader.Read(_bizTalkAppName);
            ht[_ssoItemName] = _ssoItemValue;
            SSOSettingsFileManager.SSOSettingsFileReader.Update(_bizTalkAppName, ht);

            this.Log.LogMessage("Updated SSO config item with name: {0}", _ssoItemName);

            return true;
        }
    }
}
