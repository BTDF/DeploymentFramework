// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Writes a value to nodes matching a given XPath
    /// </summary>
    public class WriteXmlValue : Task
    {
        private ITaskItem[] _xmlFilenames;
        private string _xpath;
        private string _value;

        [Required]
        public ITaskItem[] XmlFilenames
        {
            get { return _xmlFilenames; }
            set { _xmlFilenames = value; }
        }

        [Required]
        public string XPath
        {
            get { return _xpath; }
            set { _xpath = value; }
        }

        [Required]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public override bool Execute()
        {
            bool success = true;

            foreach (ITaskItem ti in _xmlFilenames)
            {
                base.Log.LogMessage(MessageImportance.Normal, "Updating '" + _xpath + "' value(s) in XML file '" + ti.ItemSpec + "'...");

                try
                {
                    XDocument doc = XDocument.Load(ti.ItemSpec, LoadOptions.PreserveWhitespace);

                    var elements = doc.XPathSelectElements(_xpath);

                    int count = elements.Count();

                    if (count > 0)
                    {
                        foreach (XElement elem in elements)
                        {
                            elem.Value = _value;
                        }

                        doc.Save(ti.ItemSpec, SaveOptions.DisableFormatting);
                        base.Log.LogMessage(MessageImportance.Normal, "Updated " + count.ToString() + " values in '" + ti.ItemSpec + "'.");
                    }
                    else
                    {
                        base.Log.LogMessage(MessageImportance.Normal, "No XPath match(es) in file '" + ti.ItemSpec + "'.");
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    base.Log.LogErrorFromException(ex, false, false, ti.ItemSpec);
                }
            }

            return success;
        }
    }
}
