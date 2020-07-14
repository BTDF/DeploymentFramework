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
    /// Creates an IIS virtual directory, or updates the configuration if it already exists.
    /// </summary>
    public class CreateVirtualDirectory : Task
    {
        private string _metabasePath;
        private string _name;
        private string _path;


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
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        // Path is of the form "<drive>:\<path>", for example, "C:\Inetpub\Wwwroot"
        [Required]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
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

            string className = site.SchemaClassName;
            if (!(className.EndsWith("Server") || className.EndsWith("VirtualDir")))
            {
                this.Log.LogError(
                    "Failed to create virtual directory {0}. Invalid MetabasePath. A virtual directory can only be created in a site or virtual directory node.",
                    _name);
                return false;
            }

            DirectoryEntry vdir = null;

            try
            {
                vdir = site.Children.Find(_name, "IIsWebVirtualDir");
            }
            catch (Exception)
            {
            }

            if (vdir != null)
            {
                base.Log.LogMessage(
                    MessageImportance.Normal,
                    "Virtual directory '{0}' already exists. Reconfiguring existing vdir.", _name);
            }
            else
            {
                vdir = site.Children.Add(_name, "IIsWebVirtualDir");
            }

            // Create and setup new virtual directory
            vdir.Properties["Path"][0] = System.IO.Path.GetFullPath(_path);
            vdir.Properties["AccessRead"][0] = true;
            vdir.Properties["AccessExecute"][0] = true;
            vdir.Properties["AccessWrite"][0] = false;
            vdir.Properties["AccessScript"][0] = true;
            vdir.Properties["EnableDirBrowsing"][0] = false;
            vdir.Properties["AuthNTLM"][0] = true;
            vdir.Properties["EnableDefaultDoc"][0] = true;
            vdir.Properties["DefaultDoc"][0] = "default.aspx";
            vdir.Properties["AspEnableParentPaths"][0] = true;
            vdir.Properties["AppFriendlyName"][0] = _name;

	        //IMPORTANT SECURITY NOTE
	        //This virtual directory is being created in high isolation mode
	        //This will cause a COM+ App to be created for this virtual directory
	        //The identity used by this COM+ App must have access the the BTS DB
	        //It is NOT recommend that you use the IWAM_MachineName acount for this.
            vdir.Invoke("AppCreate", new object[] { false });

            try
            {
                vdir.CommitChanges();
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            base.Log.LogMessage("Virtual directory '{0}' created/updated with path '{1}'.", _name, System.IO.Path.GetFullPath(_path));

            return true;
        }
    }
}
