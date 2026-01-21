// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Diagnostics;
using System.Xml;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Check that XML files have correct syntax.
    /// </summary>
    public class CheckXmlSyntax : Task
    {
        private ITaskItem[] _xmlFilenames;

        [Required]
        public ITaskItem[] XmlFilenames
        {
            get { return _xmlFilenames; }
            set { _xmlFilenames = value; }
        }

        public override bool Execute()
        {
            bool success = true;

            foreach (ITaskItem ti in _xmlFilenames)
            {
                base.Log.LogMessage(MessageImportance.Normal, "Checking syntax of XML file '" + ti.ItemSpec + "'...");

                try
                {
                    using (XmlReader reader = XmlReader.Create(ti.ItemSpec))
                    {
                        while (reader.Read())
                        {
                            ;
                        }
                    }

                    base.Log.LogMessage(MessageImportance.Normal, "Syntax of XML file '" + ti.ItemSpec + "' appears to be valid.");
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
