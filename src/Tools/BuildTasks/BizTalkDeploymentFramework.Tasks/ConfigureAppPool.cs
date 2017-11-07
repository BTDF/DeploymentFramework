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
    /// Configures the properties of an IIS application pool. Currently just sets the identity.
    /// </summary>
    public class ConfigureAppPool : Task
    {
        private string _metabasePath;
        private string _appPoolName;
        private string _userName;
        private string _password;

        [Required]
        public string MetabasePath
        {
            get { return _metabasePath; }
            set { _metabasePath = value; }
        }

        [Required]
        public string AppPoolName
        {
            get { return _appPoolName; }
            set { _appPoolName = value; }
        }

        [Required]
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        [Required]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public override bool Execute()
        {
            DirectoryEntry appPools = null;

            try
            {
                Uri u = new Uri(_metabasePath);
                appPools = new DirectoryEntry("IIS://" + u.Host + "/W3SVC/AppPools");
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            DirectoryEntry appPool = null;

            try
            {
                appPool = appPools.Children.Find(_appPoolName, "IIsApplicationPool");
            }
            catch (DirectoryServicesCOMException ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            // Create and setup new virtual directory if necessary
            appPool.Properties["AppPoolIdentityType"][0] = "3";  // Custom user
            appPool.Properties["WAMUserName"][0] = _userName;
            appPool.Properties["WAMUserPass"][0] = _password;

            try
            {
                appPool.CommitChanges();
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            base.Log.LogMessage("App pool '{0}' identity configured as user '{1}'.", _appPoolName, _userName);

            return true;
        }
    }
}
