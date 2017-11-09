// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml.XPath;

namespace DeploymentFramework.BuildTasks
{
    public class SetPropFromEnvSetting : Task
    {
        private string[] _value;
        private string _settingsFilePath;
        private string _xpath;
        private string _propertyName;
        private string _identity;

        [Output]
        public string[] Value
        {
            get { return _value; }
            set { _value = value; }
        }

        [Required]
        public string SettingsFilePath
        {
            get { return _settingsFilePath; }
            set { _settingsFilePath = value; }
        }

        [Required]
        public string XPath
        {
            get { return _xpath; }
            set { _xpath = value; }
        }

        [Required]
        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        public override bool Execute()
        {
            this.Log.LogMessage(MessageImportance.Low, "Attempting to read property {0} from settings XML file {1}...", _propertyName, _settingsFilePath);

            XPathDocument xpd = new XPathDocument(_settingsFilePath);
            XPathNavigator xpn = xpd.CreateNavigator();

            XPathNavigator setting = xpn.SelectSingleNode(_xpath);

            if (setting == null)
            {
                _value = new string[1];
                _value[0] = string.Empty;
                this.Log.LogWarning("Could not find '{0}' in the settings XML file '{1}'.", _propertyName, _settingsFilePath);
            }
            else
            {
                _value = setting.Value.Split(';');
                this.Log.LogMessage(MessageImportance.Normal, "Setting property '{0}' to value '{1}'.", _propertyName, setting.Value);
            }

            return true;
        }
    }
}
