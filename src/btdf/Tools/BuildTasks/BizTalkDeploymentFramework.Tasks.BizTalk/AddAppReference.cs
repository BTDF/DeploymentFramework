// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
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

        public AddAppReference()
        {
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
            using (BtsCatalogExplorer catalog = BizTalkCatalogExplorerFactory.GetCatalogExplorer())
            {
                Application application = catalog.Applications[_applicationName];
                if (application == null)
                {
                    this.Log.LogError("Unable to find BizTalk application '{0}'.", _applicationName);
                    return false;
                }

                foreach (ITaskItem ti in _appsToReference)
                {
                    Application appToRef = catalog.Applications[ti.ItemSpec];
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
                    catalog.SaveChanges();
                }

                this.Log.LogMessage("Finished adding application references to BizTalk application '{0}'.", _applicationName);
            }

            return true;
        }
    }
}
