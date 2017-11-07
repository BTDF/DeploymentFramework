// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.XLANGs.BizTalk.CrossProcess;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Build.Utilities;
using System.IO;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Base task class for tasks that update BizTalk's BTSNTSvc.exe.config file.
    /// </summary>
    public abstract class UpdateBTSNTSvcExeConfigBase : Task
    {
        /// <summary>
        /// Override in derived classes to update the xlangs configuration loaded from the config file.
        /// </summary>
        /// <param name="config"></param>
        protected abstract void UpdateConfiguration(Configuration config);

        public override bool Execute()
        {
            string configFilePath = GetBizTalkConfigFilePath(true);
            if (File.Exists(configFilePath))
            {
                UpdateBizTalkConfigFile(configFilePath);
            }

            configFilePath = GetBizTalkConfigFilePath(false);
            if (File.Exists(configFilePath))
            {
                UpdateBizTalkConfigFile(configFilePath);
            }

            return true;
        }

        private void UpdateBizTalkConfigFile(string configFilePath)
        {
            this.Log.LogMessage("Updating {0}...", configFilePath);

            XmlDocument btsConfig = new XmlDocument();
            btsConfig.Load(configFilePath);

            // Deserialize the configuration XML
            Configuration config = GetXlangsSection(btsConfig);

            // Call the virtual method to allow derived classes to update the configuration
            UpdateConfiguration(config);

            // Store the updated configuration back into the config XML document
            PutXlangsSection(btsConfig, config);

            // Save to disk
            CreateConfigFileBackup(configFilePath);
            btsConfig.Save(configFilePath);

            this.Log.LogMessage("Saved updated BizTalk configuration file.");
        }

        protected string GetBizTalkConfigFilePath(bool get32Bit)
        {
            string path = null;

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\BizTalk Server\\3.0"))
            {
                path = (string)rk.GetValue("InstallPath");
            }

            if (string.IsNullOrEmpty(path))
            {
                this.Log.LogError(
                    "BizTalk does not appear to be installed (missing BizTalk Server\\3.0\\InstallPath registry value).");
                return null;
            }

            if (get32Bit)
            {
                return System.IO.Path.Combine(path, "BTSNTSvc.exe.config");
            }
            else
            {
                return System.IO.Path.Combine(path, "BTSNTSvc64.exe.config");
            }
        }

        protected void CreateConfigFileBackup(string configFilePath)
        {
            string configFileBackupPath = configFilePath + ".bak";

            System.IO.File.Copy(configFilePath, configFileBackupPath, true);
        }

        /// <summary>
        /// Load and deserialize the xlangs configuration section, or create a new one if it does not already exist.
        /// </summary>
        /// <param name="btsConfig"></param>
        /// <returns></returns>
        protected Configuration GetXlangsSection(XmlDocument btsConfig)
        {
            XmlElement configSections = btsConfig.SelectSingleNode("//configSections") as XmlElement;

            if (configSections == null)
            {
                configSections = btsConfig.CreateElement("configSections");
                btsConfig.DocumentElement.PrependChild(configSections);
            }

            XmlElement xlangsSection = configSections.SelectSingleNode("section[@name='xlangs']") as XmlElement;

            if (xlangsSection == null)
            {
                xlangsSection = btsConfig.CreateElement("section");
                xlangsSection.SetAttribute("name", "xlangs");
                xlangsSection.SetAttribute("type", "Microsoft.XLANGs.BizTalk.CrossProcess.XmlSerializationConfigurationSectionHandler, Microsoft.XLANGs.BizTalk.CrossProcess");
                configSections.AppendChild(xlangsSection);
            }

            XmlElement xlangs = btsConfig.SelectSingleNode("//xlangs") as XmlElement;

            if (xlangs == null)
            {
                xlangs = btsConfig.CreateElement("xlangs");
                btsConfig.DocumentElement.AppendChild(xlangs);
            }

            System.Configuration.IConfigurationSectionHandler sh = new XmlSerializationConfigurationSectionHandler();

            return sh.Create(this, null, xlangs) as Configuration;
        }

        /// <summary>
        /// Place an updated xlangs configuration section back into the original BizTalk config XML document.
        /// </summary>
        /// <param name="btsConfig"></param>
        /// <param name="config"></param>
        protected void PutXlangsSection(XmlDocument btsConfig, Configuration config)
        {
            XmlElement xlangs = btsConfig.SelectSingleNode("//xlangs") as XmlElement;

            XmlSerializer xmls = new XmlSerializer(typeof(Configuration));

            StringBuilder configXml = new StringBuilder();

            // Serialize the xlangs configuration to XML in the configXml StringBuilder
            using (XmlWriter xmlw = XmlWriter.Create(configXml))
            {
                xmls.Serialize(xmlw, config);
            }

            // Load the just-serialized XML into a new XmlDocument
            XmlDocument xlangsConfig = new XmlDocument();
            xlangsConfig.LoadXml(configXml.ToString());

            // Import the xlangs configuration node into the BizTalk config XML document
            XmlElement importedXlangs = btsConfig.ImportNode(xlangsConfig.DocumentElement, true) as XmlElement;

            if (xlangs.ChildNodes.Count > 0)
            {
                xlangs.ReplaceChild(importedXlangs, xlangs.ChildNodes[0]);
            }
            else
            {
                xlangs.AppendChild(importedXlangs);
            }
        }
    }
}
