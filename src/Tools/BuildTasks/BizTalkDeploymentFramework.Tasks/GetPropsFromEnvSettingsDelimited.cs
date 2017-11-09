// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Xml.Linq;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace DeploymentFramework.BuildTasks
{
    public class GetPropsFromEnvSettingsDelimited : Task
    {
        private string[] _value;
        private string _settingsFilePath;
        private string _xpathTemplate;
        private ITaskItem[] _propertyNames;

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
        public string XPathTemplate
        {
            get { return _xpathTemplate; }
            set { _xpathTemplate = value; }
        }

        [Required]
        public ITaskItem[] PropertyNames
        {
            get { return _propertyNames; }
            set { _propertyNames = value; }
        }
	
        public override bool Execute()
        {
            this.Log.LogMessage(MessageImportance.Low, "Attempting to read property values from environment settings XML file {0}...", _settingsFilePath);

            List<string> properties = new List<string>();

            // Load the settings XML file
            XDocument xpd = XDocument.Load(_settingsFilePath);

            // For each property name...
            foreach (ITaskItem propertyName in _propertyNames)
            {
                // Build and run an XPath to get the element
                string xpath = _xpathTemplate.Replace("@@NAME@@", propertyName.ItemSpec);

                XElement setting = ((IEnumerable)xpd.XPathEvaluate(xpath)).Cast<XElement>().FirstOrDefault();

                if (setting != null)
                {
                    properties.Add(propertyName + "=" + setting.Value);
                }
            }

            // Prepare the output variable
            _value = new string[1];

            if (_propertyNames.Count() == 0)
            {
                _value[0] = string.Empty;
            }
            else
            {
                _value = properties.ToArray();
            }

            return true;
        }
    }
}
