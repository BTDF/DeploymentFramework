using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using System.Xml;

namespace BizTalk.NAnt.Tasks
{
    /// <summary>
    /// Loads a BizTalk binding XML file, prepends the specified string to all port names and saves the file.
    /// </summary>
    [TaskName("prependToBindingPortNames")]
    public class PrependToBindingPortNames : Task
    {
        private enum PortType
        {
            Send,
            Receive
        }

        private string _stringToPrepend;
        private string _bindingFilePath;

        [TaskAttribute("stringToPrepend")]
        public string StringToPrepend
        {
            get { return _stringToPrepend; }
            set { _stringToPrepend = value; }
        }

        [TaskAttribute("bindingFilePath")]
        public string BindingFilePath
        {
            get { return _bindingFilePath; }
            set { _bindingFilePath = value; }
        }

        protected override void ExecuteTask()
        {
            XmlDocument bindingDoc = new XmlDocument();
            bindingDoc.Load(_bindingFilePath);

            XmlNodeList nodesToUpdate = null;

            this.Log(Level.Info, "Prepending '{0}' to SendPortRef names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//SendPortRef");
            UpdatePortRefs(nodesToUpdate, PortType.Send, bindingDoc);

            this.Log(Level.Info, "Prepending '{0}' to ReceivePortRef names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//ReceivePortRef");
            UpdatePortRefs(nodesToUpdate, PortType.Receive, bindingDoc);

            this.Log(Level.Info, "Prepending '{0}' to SendPort names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//SendPort");
            UpdateNames(nodesToUpdate);

            this.Log(Level.Info, "Prepending '{0}' to ReceivePort names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//ReceivePort");
            UpdateNames(nodesToUpdate);

            this.Log(Level.Info, "Prepending '{0}' to ReceiveLocation names...", _stringToPrepend);
            nodesToUpdate = bindingDoc.SelectNodes("//ReceiveLocation");
            UpdateNames(nodesToUpdate);

            bindingDoc.Save(_bindingFilePath);
        }

        private void UpdateNames(XmlNodeList nodesToUpdate)
        {
            foreach (XmlElement nodeToUpdate in nodesToUpdate)
            {
                XmlAttribute attrToUpdate = nodeToUpdate.Attributes["Name"];

                if (attrToUpdate != null)
                {
                    attrToUpdate.Value = string.Format("{0}_{1}", _stringToPrepend, attrToUpdate.Value);
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
                        attrToUpdate.Value = string.Format("{0}_{1}", _stringToPrepend, attrToUpdate.Value);
                    }
                }
            }
        }
    }
}
