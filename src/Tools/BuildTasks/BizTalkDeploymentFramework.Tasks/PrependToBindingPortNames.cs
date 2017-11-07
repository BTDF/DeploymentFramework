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
using System.Xml;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Loads a BizTalk binding XML file, prepends the specified string to all port names and saves the file.
    /// </summary>
    public class PrependToBindingPortNames : Task
    {
        private enum PortType
        {
            Send,
            Receive
        }

        private string _stringToPrepend;
        private string _bindingFilePath;

        [Required]
        public string StringToPrepend
        {
            get { return _stringToPrepend; }
            set { _stringToPrepend = value; }
        }

        [Required]
        public string BindingFilePath
        {
            get { return _bindingFilePath; }
            set { _bindingFilePath = value; }
        }

        public override bool Execute()
        {
            XmlDocument bindingDoc = new XmlDocument();
            bindingDoc.Load(_bindingFilePath);

            XmlNodeList nodesToUpdate = null;

            this.Log.LogMessage(MessageImportance.Normal, "Prepending '{0}' to SendPortRef names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//SendPortRef");
            UpdatePortRefs(nodesToUpdate, PortType.Send, bindingDoc);

            this.Log.LogMessage(MessageImportance.Normal, "Prepending '{0}' to ReceivePortRef names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//ReceivePortRef");
            UpdatePortRefs(nodesToUpdate, PortType.Receive, bindingDoc);

            this.Log.LogMessage(MessageImportance.Normal, "Prepending '{0}' to SendPort names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//SendPort");
            UpdateNames(nodesToUpdate);

            this.Log.LogMessage(MessageImportance.Normal, "Prepending '{0}' to ReceivePort names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//ReceivePort");
            UpdateNames(nodesToUpdate);

            this.Log.LogMessage(MessageImportance.Normal, "Prepending '{0}' to ReceiveLocation names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//ReceiveLocation");
            UpdateNames(nodesToUpdate);

            bindingDoc.Save(_bindingFilePath);

            return true;
        }


        private void UpdateNames(XmlNodeList nodesToUpdate)
        {
            foreach (XmlElement nodeToUpdate in nodesToUpdate)
            {
                XmlAttribute attrToUpdate = nodeToUpdate.Attributes["Name"];

                if (attrToUpdate != null)
                {
                    if (!attrToUpdate.Value.StartsWith(_stringToPrepend + "_"))
                    {
                        attrToUpdate.Value = string.Format("{0}_{1}", _stringToPrepend, attrToUpdate.Value);
                    }
                }
            }
        }

        private void UpdatePortRefs(XmlNodeList nodesToUpdate, PortType portType, XmlDocument bindingDoc)
        {
            string definitionNodeName = null;

            if (portType == PortType.Receive)
            {
                definitionNodeName = "ReceivePort";
            }
            else
            {
                definitionNodeName = "SendPort";
            }

            foreach (XmlElement nodeToUpdate in nodesToUpdate)
            {
                XmlAttribute attrToUpdate = nodeToUpdate.Attributes["Name"];

                if (attrToUpdate != null)
                {
                    // If the send/receive port referred to by the port ref is not defined in this binding file,
                    // leave the name unchanged.  The reference is probably to a port in another application.

                    XmlNode portDefinition =
                        bindingDoc.SelectSingleNode(string.Format("//{0}[@Name='{1}']", definitionNodeName, attrToUpdate.Value));

                    if (portDefinition != null)
                    {
                        if (!attrToUpdate.Value.StartsWith(_stringToPrepend + "_"))
                        {
                            attrToUpdate.Value = string.Format("{0}_{1}", _stringToPrepend, attrToUpdate.Value);
                        }
                    }
                }
            }
        }
    }
}
