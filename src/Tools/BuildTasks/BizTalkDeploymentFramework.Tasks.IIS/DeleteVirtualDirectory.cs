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
using System.IO;
using System.Management;
using System.DirectoryServices;

namespace DeploymentFramework.BuildTasks
{
    public class DeleteVirtualDirectory : Task
    {
        private string _metabasePath;
        private string _name;

        // MetabasePath is of the form "IIS://<servername>/<service>/<siteID>/Root[/<vdir>]"
        // for example "IIS://localhost/W3SVC/1/Root" 
        [Required]
        public string MetabasePath
        {
            get { return _metabasePath; }
            set { _metabasePath = value; }
        }

        [Required]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public override bool Execute()
        {
            DirectoryEntry de = null;

            try
            {
                de = new DirectoryEntry(_metabasePath);
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            foreach (DirectoryEntry de2 in de.Children)
            {
                if (de2.SchemaClassName == "IIsWebVirtualDir" && string.Compare(de2.Name, _name, true) == 0)
                {
                    de2.Invoke("AppDeleteRecursive", new object[] {});
                    try
                    {
                        de2.CommitChanges();
                    }
                    catch (Exception ex)
                    {
                        this.Log.LogErrorFromException(ex, true);
                        return false;
                    }

                    de.Invoke("Delete", new object[] { "IIsWebVirtualDir", _name });
                    try
                    {
                        de.CommitChanges();
                    }
                    catch (Exception ex)
                    {
                        this.Log.LogErrorFromException(ex, true);
                        return false;
                    }

                    base.Log.LogMessage("Virtual directory '{0}' deleted.", _name);

                    return true;
                }
            }

            base.Log.LogMessage(MessageImportance.Normal, "Virtual directory '{0}' does not exist.", _name);

            return true;
        }
    }
}
