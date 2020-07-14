// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Iterates through an ItemGroup containing the names and locations of BizTalk rule policy files.
    /// For each file, load the rule policy XML and locate the FIRST ruleset. Extract the ruleset name
    /// and the most recently modified version entry and store them into the item metadata in the
    /// original ItemGroup.
    /// </summary>
    public class PopulateRulePoliciesMetadata : Task
    {
        private bool _reverse;
        private ITaskItem[] _itemGroup;
        private ITaskItem[] _outputItemGroup;

        public ITaskItem[] PolicyVocabFiles
        {
            get { return _itemGroup; }
            set { _itemGroup = value; }
        }

        public bool Reverse
        {
            get { return _reverse; }
            set { _reverse = value; }
        }

        [Output]
        public ITaskItem[] PolicyVocabWithMetadata
        {
            get { return _outputItemGroup; }
        }

        public override bool Execute()
        {
            if (_itemGroup == null)
            {
                return true;
            }

            base.Log.LogMessage("Populating metadata in ItemGroup from policy/vocabulary files content...");

            if (_reverse)
            {
                Array.Reverse(_itemGroup);
            }

            List<TaskItem> newTaskItems = new List<TaskItem>();

            // For each policy/vocab file...
            for (int index = 0; index < _itemGroup.Length; index++)
            {
                ITaskItem currentTaskItem = _itemGroup[index];
                string policyVocabFilePath = currentTaskItem.ItemSpec;

                List<TaskItem> newItems = ProcessPolicyVocabFile(policyVocabFilePath);

                if (newItems == null)
                {
                    return false;
                }

                newTaskItems.AddRange(newItems);
            }

            _outputItemGroup = newTaskItems.ToArray();

            base.Log.LogMessage("Finished populating metadata in ItemGroup from policy/vocabulary files content.");

            return true;
        }

        private XmlElement LocateNewestVersionNode(XmlElement rulesetVocabElement, XmlNamespaceManager xmlNs)
        {
            DateTime newestDateTime = new DateTime();
            XmlElement newestVersionNode = null;

            XmlNodeList versionNodes = rulesetVocabElement.SelectNodes("br:version", xmlNs);

            foreach (XmlElement versionNode in versionNodes)
            {
                DateTime dt = DateTime.Parse(versionNode.Attributes["date"].Value, CultureInfo.InvariantCulture);

                if (dt > newestDateTime)
                {
                    newestVersionNode = versionNode;
                    newestDateTime = dt;
                }
            }

            return newestVersionNode;
        }

        private List<TaskItem> ProcessPolicyVocabFile(string policyFilePath)
        {
            base.Log.LogMessage("  Working on '{0}'...", policyFilePath);

            XmlDocument policyDocument = new XmlDocument();

            try
            {
                // Load the policy file into an XmlDocument
                policyDocument.Load(policyFilePath);
            }
            catch (Exception ex)
            {
                base.Log.LogError("    Failed reading policy XML file: {0}", ex.Message);
                return null;
            }

            XmlNamespaceManager xmlNs = new XmlNamespaceManager(policyDocument.NameTable);
            xmlNs.AddNamespace("br", "http://schemas.microsoft.com/businessruleslanguage/2002");

            List<TaskItem> newTaskItems = new List<TaskItem>();

            // Find all <vocabulary> elements
            XmlNodeList vocabularyElements = policyDocument.SelectNodes("/br:brl/br:vocabulary", xmlNs);

            if (vocabularyElements.Count > 0)
            {
                newTaskItems.AddRange(CreateTaskItemsFromElements(policyFilePath, vocabularyElements, xmlNs));
            }

            // Find all <ruleset> elements
            XmlNodeList ruleSetElements = policyDocument.SelectNodes("/br:brl/br:ruleset", xmlNs);

            if (ruleSetElements.Count > 0)
            {
                newTaskItems.AddRange(CreateTaskItemsFromElements(policyFilePath, ruleSetElements, xmlNs));
            }

            if (vocabularyElements.Count == 0 && ruleSetElements.Count == 0)
            {
                base.Log.LogError("    XML file does not contain any <vocabulary> or <ruleset> elements.");
                return null;
            }

            return newTaskItems;
        }

        private List<TaskItem> CreateTaskItemsFromElements(
            string policyFilePath, XmlNodeList ruleSetVocabElements, XmlNamespaceManager xmlNs)
        {
            List<TaskItem> newTaskItems = new List<TaskItem>();

            foreach (XmlElement ruleSetVocabElement in ruleSetVocabElements)
            {
                // Save the name from the <ruleset> or <vocabulary>. This will be our policy name.
                string ruleSetVocabName = ruleSetVocabElement.Attributes["name"].Value;

                // Locate the <version> node with the most recent date attribute value
                XmlElement newestVersionNode = LocateNewestVersionNode(ruleSetVocabElement, xmlNs);

                string policyVersion = newestVersionNode.Attributes["major"].Value + "." + newestVersionNode.Attributes["minor"].Value;

                TaskItem newTaskItem = new TaskItem(ruleSetVocabName + "." + policyVersion);

                // Set the name and version into the new TaskItem
                newTaskItem.SetMetadata("SourcePath", policyFilePath);
                newTaskItem.SetMetadata("PolicyVersion", policyVersion);
                newTaskItem.SetMetadata("PolicyName", ruleSetVocabName);
                newTaskItem.SetMetadata("Luid", string.Format("RULE/{0}/{1}", ruleSetVocabName, policyVersion));

                newTaskItems.Add(newTaskItem);

                base.Log.LogMessage(
                    "    Added item [PolicyName:'{0}'; PolicyVersion:'{1}']",
                    newTaskItem.GetMetadata("PolicyName"),
                    newTaskItem.GetMetadata("PolicyVersion"));
            }

            return newTaskItems;
        }
    }
}
