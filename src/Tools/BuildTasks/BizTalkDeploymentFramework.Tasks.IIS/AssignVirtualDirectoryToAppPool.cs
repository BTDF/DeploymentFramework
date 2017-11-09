// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Management;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Assigns an IIS directory into an application pool.
    /// </summary>
    public class AssignVirtualDirectoryToAppPool : Task
    {
        private string _metabasePath;
        private string _vdirname;
        private string _appPoolName;

        // MetabasePath is of the form "IIS://<servername>/<service>/<siteID>/Root[/<vdir>]"
        // for example "IIS://localhost/W3SVC/1/Root" 
        [Required]
        public string MetabasePath
        {
            get { return _metabasePath; }
            set { _metabasePath = value; }
        }

        // Name is of the form "<name>", for example, "MyNewVDir"
        [Required]
        public string VDirName
        {
            get { return _vdirname; }
            set { _vdirname = value; }
        }

        [Required]
        public string AppPoolName
        {
            get { return _appPoolName; }
            set { _appPoolName = value; }
        }

        public override bool Execute()
        {
            DirectoryEntry site = null;

            try
            {
                site = new DirectoryEntry(_metabasePath);
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            DirectoryEntry vdir = null;

            try
            {
                vdir = site.Children.Find(_vdirname, "IIsWebVirtualDir");
            }
            catch (DirectoryServicesCOMException ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            string className = site.SchemaClassName;
            if (!className.EndsWith("VirtualDir"))
            {
                this.Log.LogError(
                    "Failed to assign AppPool to virtual directory {0}. Invalid MetabasePath or vdir Name. An app pool can only be assigned to a virtual directory.",
                    _vdirname);
                return false;
            }

            // Create and setup new app pool if necessary
            object[] param = { 0, _appPoolName, true };
            vdir.Invoke("AppCreate3", param);
            vdir.Properties["AppIsolated"][0] = "2";

            try
            {
                vdir.CommitChanges();
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            base.Log.LogMessage("App pool '{0}' assigned to virtual directory '{1}'.", _appPoolName, _vdirname);

            return true;
        }
    }
}
