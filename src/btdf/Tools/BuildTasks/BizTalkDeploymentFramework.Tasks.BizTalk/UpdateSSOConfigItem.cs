// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
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
			get{ return _ssoItemName; }
			set
			{
				_ssoItemName = value;
			}
		}

		[Required]
		public string SSOItemValue
		{
			get{ return _ssoItemValue; }
			set
			{
				_ssoItemValue = value;
			}
		}

        public override bool Execute()
        {
            SSOSettingsFileManager.SSOSettingsManager.WriteSetting(_bizTalkAppName, _ssoItemName, _ssoItemValue);

            this.Log.LogMessage("Updated SSO config item with name: {0}", _ssoItemName);

            return true;
        }
    }
}
