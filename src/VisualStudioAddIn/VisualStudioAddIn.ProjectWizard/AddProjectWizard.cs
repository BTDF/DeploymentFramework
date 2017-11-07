// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.Win32;
using System.Reflection;
using System.ComponentModel;

namespace DeploymentFramework.VisualStudioAddIn.ProjectWizard
{
    public class AddProjectWizard : IWizard
    {
        #region IWizard Members

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            DTE2 dte = automationObject as DTE2;

            if (dte == null)
            {
                MessageBox.Show(
                    "Cannot convert automation object to DTE2.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!dte.Solution.IsOpen)
            {
                MessageBox.Show(
                    "Please open your BizTalk solution, then use the Add New Project dialog on the solution to add this project.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string destinationPath = replacementsDictionary["$destinationdirectory$"];

            if (string.IsNullOrEmpty(destinationPath))
            {
                MessageBox.Show(
                    "Cannot determine destination directory ($destinationdirectory$ is missing).",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string btdfInstallDir =
                (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\DeploymentFrameworkForBizTalk", "InstallPath", null);

            if (string.IsNullOrEmpty(btdfInstallDir))
            {
                MessageBox.Show(
                    "Cannot find Deployment Framework registry key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DeploymentOptions options = new DeploymentOptions();

            OptionsForm optionsFrm = new OptionsForm(options);
            if (optionsFrm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string templateDir = Path.Combine(btdfInstallDir, @"Developer\ProjectTemplate");

            string[] templateFiles = Directory.GetFiles(templateDir, "*.*", SearchOption.AllDirectories);

            try
            {
                CopyDirectory(templateDir, destinationPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to copy Deployment Framework template files to destination folder: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string solutionName = Path.GetFileNameWithoutExtension(dte.Solution.FileName);
            string projectFileName = "Deployment.btdfproj";
            string projectFilePath = Path.Combine(destinationPath, projectFileName);

            Dictionary<string, string> replacements = new Dictionary<string, string>();
            replacements.Add("[PROJECTNAME]", solutionName);

            ReplaceInTextFile(destinationPath, "InstallWizard.xml", replacements, Encoding.UTF8);
            ReplaceInTextFile(destinationPath, "UnInstallWizard.xml", replacements, Encoding.UTF8);

            replacements.Add("[GUID1]", Guid.NewGuid().ToString());
            replacements.Add("[GUID2]", Guid.NewGuid().ToString());
            ReplaceInTextFile(destinationPath, projectFileName, replacements, Encoding.UTF8);

            UpdateProjectFile(projectFilePath, options, optionsFrm.WritePropertiesOnlyWhenNonDefault);

            try
            {
                dte.ExecuteCommand("File.OpenFile", '"' + projectFilePath + '"');
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to open .btdfproj file in editor.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);                
            }

            MessageBox.Show(
                "A default Deployment Framework for BizTalk project has been configured in " + destinationPath + ". You must edit " + projectFileName + " to configure your specific deployment requirements.",
                "Project Created",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        #endregion

        private static void ReplaceInTextFile(
            string filePath, string fileName, Dictionary<string, string> replacements, Encoding encodingMode)
        {
            string combinedFilePath = null;
            string fileContents = null;

            combinedFilePath = Path.Combine(filePath, fileName);
            fileContents = File.ReadAllText(combinedFilePath, encodingMode);

            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                fileContents = fileContents.Replace(replacement.Key, replacement.Value);
            }

            File.WriteAllText(combinedFilePath, fileContents, encodingMode);
        }

        // Copy directory structure recursively
        // From a CodeProject article by Richard Lopes  
        private static void CopyDirectory(string src, string dest)
        {
            string[] files;

            if (dest[dest.Length - 1] != Path.DirectorySeparatorChar)
            {
                dest += Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            files = Directory.GetFileSystemEntries(src);

            foreach (string element in files)
            {
                // Sub directories
                if (Directory.Exists(element))
                {
                    CopyDirectory(element, dest + Path.GetFileName(element));
                }
                // Files in directory
                else
                {
                    File.Copy(element, dest + Path.GetFileName(element), true);
                }
            }
        }

        private void UpdateProjectFile(string projectFilePath, DeploymentOptions options, bool writeOnlyWhenNonDefault)
        {
            XmlDocument projectXml = new XmlDocument();
            projectXml.Load(projectFilePath);

            XmlNamespaceManager xnm = new XmlNamespaceManager(projectXml.NameTable);
            xnm.AddNamespace(string.Empty, "http://schemas.microsoft.com/developer/msbuild/2003");
            xnm.AddNamespace("ns0", "http://schemas.microsoft.com/developer/msbuild/2003");

            XmlElement projectElement = projectXml.SelectSingleNode("/ns0:Project", xnm) as XmlElement;
            string bizTalkProductName = GetBizTalkProductName();

            if (string.Compare(bizTalkProductName, "Microsoft BizTalk Server 2010", true) == 0
                || string.Compare(bizTalkProductName, "Microsoft BizTalk Server 2013", true) == 0)
            {
                projectElement.SetAttribute("ToolsVersion", "4.0");
            }

            XmlElement generalPropertyGroup = projectXml.SelectSingleNode("/ns0:Project/ns0:PropertyGroup[1]", xnm) as XmlElement;

            Type doType = options.GetType();
            PropertyInfo[] doProperties = doType.GetProperties();

            foreach (PropertyInfo pi in doProperties)
            {
                if (pi.PropertyType.Equals(typeof(bool)))
                {
                    if (writeOnlyWhenNonDefault)
                    {
                        object[] dvAttribute = pi.GetCustomAttributes(typeof(DefaultValueAttribute), false);
                        DefaultValueAttribute dva = dvAttribute[0] as DefaultValueAttribute;
                        bool defaultValue = (bool)dva.Value;

                        bool propertyValue = (bool)pi.GetValue(options, null);

                        if (defaultValue != propertyValue)
                        {
                            WriteElementText(projectXml, xnm, generalPropertyGroup, pi.Name, propertyValue);
                        }
                    }
                    else
                    {
                        WriteElementText(projectXml, xnm, generalPropertyGroup, pi.Name, (bool)pi.GetValue(options, null));
                    }
                }
            }


            projectXml.Save(projectFilePath);
        }

        private void WriteElementText<T>(
            XmlDocument projectXml, XmlNamespaceManager xnm, XmlElement propertyGroup, string elementName, T elementValue)
        {
            XmlElement xe = propertyGroup.SelectSingleNode("ns0:" + elementName, xnm) as XmlElement;

            if (xe == null)
            {
                xe = projectXml.CreateElement(string.Empty, elementName, xnm.DefaultNamespace);
                xe.InnerText = elementValue.ToString();
                propertyGroup.AppendChild(xe);
            }
            else
            {
                xe.InnerText = elementValue.ToString();
            }
        }

        private string GetBizTalkProductName()
        {
            string bizTalkProduct =
                (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\BizTalk Server\3.0", "ProductName", null);

            if (string.IsNullOrEmpty(bizTalkProduct))
            {
                MessageBox.Show(
                    "Cannot find BizTalk Server install registry key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return bizTalkProduct;
        }
    }
}
