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
    /// Adds or removes an IIS web service extension.
    /// </summary>
    public class ConfigureWebServiceExtension : Task
    {
        private string _metabasePath;
        private string _extensionFile;
        private string _extensionName;
        private string _remove = "false";

        // MetabasePath is of the form "IIS://<servername>/<service>/<siteID>/Root[/<vdir>]"
        // for example "IIS://localhost/W3SVC/1/Root" 
        [Required]
        public string MetabasePath
        {
            get { return _metabasePath; }
            set { _metabasePath = value; }
        }

        [Required]
        public string ExtensionFile
        {
            get { return _extensionFile; }
            set { _extensionFile = value; }
        }

        [Required]
        public string ExtensionName
        {
            get { return _extensionName; }
            set { _extensionName = value; }
        }

        public string Remove
        {
            get { return _remove; }
            set { _remove = value; }
        }

        public override bool Execute()
        {
            DirectoryEntry webService = null;

            bool remove = bool.Parse(_remove);

            try
            {
                Uri u = new Uri(_metabasePath);
                webService = new DirectoryEntry("IIS://" + u.Host + "/W3SVC");
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            if (!remove)
            {
                try
                {
                    webService.Invoke(
                        "AddExtensionFile", new object[] { _extensionFile, true, _extensionName, true, _extensionName });
                }
                catch (Exception)
                {
                    base.Log.LogError("Failed to add IIS extension '{0}'. It may have been previously added.", _extensionFile);
                    return false;
                }
            }
            else
            {
                try
                {
                    webService.Invoke("DeleteExtensionFileRecord", new object[] { _extensionFile });
                }
                catch (Exception)
                {
                    base.Log.LogError("Failed to delete IIS extension '{0}'. It may have been previously removed.", _extensionFile);
                    return false;
                }
            }

            try
            {
                webService.CommitChanges();
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }

            if (!remove)
            {
                base.Log.LogMessage("IIS extension '{0}' added with path '{1}'.", _extensionName, _extensionFile);
            }
            else
            {
                base.Log.LogMessage("IIS extension '{0}' removed with path '{1}'.", _extensionName, _extensionFile);
            }

            return true;
        }
    }
}
