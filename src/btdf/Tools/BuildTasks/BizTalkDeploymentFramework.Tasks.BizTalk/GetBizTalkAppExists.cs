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
    public class GetBizTalkAppExists : Task
    {
        private bool _appExists;
        private string _applicationName;

        public GetBizTalkAppExists()
        {
        }

        [Required]
        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        [Output]
        public bool AppExists
        {
            get { return _appExists; }
            set { _appExists = value; }
        }

        public override bool Execute()
        {
            this.Log.LogMessage("Checking for existence of BizTalk application '{0}'...", _applicationName);

            using (BtsCatalogExplorer catalog = BizTalkCatalogExplorerFactory.GetCatalogExplorer())
            {
                Application application = catalog.Applications[_applicationName];
                _appExists = (application != null);
            }

            if (_appExists)
            {
                this.Log.LogMessage("Found BizTalk application '{0}'.", _applicationName);
            }
            else
            {
                this.Log.LogMessage("Did not find BizTalk application '{0}'.", _applicationName);
            }

            return true;
        }
    }
}
