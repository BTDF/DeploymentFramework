using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.BizTalk.ExplorerOM;
using BizTalk.NAnt.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace BizTalk.NAnt.Tasks
{
    [TaskName("addappreference")]
    class AddAppReference : Task
    {
        private string _applicationName;
        private string _appsToReference;
        BtsCatalogExplorer _catalog = null;

        public AddAppReference()
        {
            // connect to the BizTalk configuration database that corresponds to our group membership.
            _catalog = new BtsCatalogExplorer();
            _catalog.ConnectionString = string.Format("Server={0};Initial Catalog={1};Integrated Security=SSPI;",
               BizTalkGroupInfo.GroupDBServerName,
               BizTalkGroupInfo.GroupMgmtDBName);
        }

        [TaskAttribute("appstoreference")]
        [StringValidator(AllowEmpty = false)]
        public string AppToReference
        {
            get { return _appsToReference; }
            set { _appsToReference = value; }
        }

        [TaskAttribute("application")]
        [StringValidator(AllowEmpty = false)]
        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        protected override void ExecuteTask()
        {
            Application application = _catalog.Applications[_applicationName];
            if (application == null)
                throw (new BuildException("Unable to find application '" + _applicationName + "'.", this.Location));

            string[] apps = _appsToReference.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < apps.GetLength(0); i++)
            {
                Application appToRef = _catalog.Applications[apps[i]];
                if (appToRef == null)
                    throw (new BuildException("Unable to find BizTalk application '" + apps[i] + "' to reference from application '" + _applicationName + "'.", this.Location));

                application.AddReference(appToRef);
            }

            if (apps.GetLength(0) > 0)
                _catalog.SaveChanges();
        }
    }
}
