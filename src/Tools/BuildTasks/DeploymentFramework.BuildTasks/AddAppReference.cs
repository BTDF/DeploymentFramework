// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.BizTalk.ExplorerOM;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace DeploymentFramework.BuildTasks
{
    public class AddAppReference : Task
    {
        private string _applicationName;
        private ITaskItem[] _appsToReference;
        BtsCatalogExplorer _catalog = null;

        public AddAppReference()
        {
            // connect to the BizTalk configuration database that corresponds to our group membership.
            _catalog = new BtsCatalogExplorer();
            _catalog.ConnectionString = string.Format("Server={0};Initial Catalog={1};Integrated Security=SSPI;",
               BizTalkGroupInfo.GroupDBServerName,
               BizTalkGroupInfo.GroupMgmtDBName);
        }

        [Required]
        public ITaskItem[] AppsToReference
        {
            get { return _appsToReference; }
            set { _appsToReference = value; }
        }

        [Required]
        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool Execute()
        {
            Application application = _catalog.Applications[_applicationName];
            if (application == null)
            {
                this.Log.LogError("Unable to find BizTalk application '{0}'.", _applicationName);
                return false;
            }

            foreach (ITaskItem ti in _appsToReference)
            {
                Application appToRef = _catalog.Applications[ti.ItemSpec];
                if (appToRef == null)
                {
                    this.Log.LogError("Unable to find BizTalk application '{0}' to reference from application '{1}'.", ti.ItemSpec, _applicationName);
                    return false;
                }

                this.Log.LogMessage("Adding reference to BizTalk application '{0}' from BizTalk application '{1}'.", _applicationName, ti.ItemSpec);
                application.AddReference(appToRef);
            }

            if (_appsToReference.Length > 0)
            {
                _catalog.SaveChanges();
            }

            this.Log.LogMessage("Finished adding application references to BizTalk application '{0}'.", _applicationName);

            return true;
        }
    }
}
