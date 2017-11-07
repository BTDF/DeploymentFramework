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
    public class GetBizTalkAppExists : Task
    {
        private bool _appExists;
        private string _applicationName;
        private BtsCatalogExplorer _catalog = null;

        public GetBizTalkAppExists()
        {
            // connect to the BizTalk configuration database that corresponds to our group membership.
            _catalog = new BtsCatalogExplorer();
            _catalog.ConnectionString = string.Format("Server={0};Initial Catalog={1};Integrated Security=SSPI;",
               BizTalkGroupInfo.GroupDBServerName,
               BizTalkGroupInfo.GroupMgmtDBName);
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
            Application application = _catalog.Applications[_applicationName];
            _appExists = (application != null);

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
