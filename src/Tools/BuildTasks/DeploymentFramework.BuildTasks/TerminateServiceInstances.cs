// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.BizTalk.Operations;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace DeploymentFramework.BuildTasks
{
    public class TerminateServiceInstances : Task
    {
        private BizTalkOperations _operations;
        private string _applicationName;

        public TerminateServiceInstances()
        {
            // connect to the BizTalk configuration database that corresponds to our group membership.
            _operations = new BizTalkOperations(BizTalkGroupInfo.GroupDBServerName, BizTalkGroupInfo.GroupMgmtDBName);
        }

        /// <summary>
        /// The application to remove running or suspended instances for.  "*" will remove
        /// all running or suspended instances for all applications.
        /// </summary>
        [Required]
        public string Application
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(_applicationName))
            {
                base.Log.LogError("Application property must be set");
                return false;
            }

            int terminatedServiceCount = 0;
            bool success = true;
            bool removeAll = (_applicationName == "*");

            base.Log.LogMessage(
                MessageImportance.Normal,
                "Attempting to terminate all service instances for BizTalk app '{0}'...", removeAll ? "[ALL APPS]" : _applicationName);

            // Get all service instances in the message box.
            foreach (Instance instance in _operations.GetServiceInstances())
            {
                // We will only deal with instances that we can determine 
                // which application it belongs to.
                MessageBoxServiceInstance mbsi = instance as MessageBoxServiceInstance;

                if (mbsi != null)
                {
                    // Only terminate if the service instance application matches the 
                    // application we are intrested in.  "*" will match all applications.
                    bool removeThisInstance = ((removeAll == true) || (string.Compare(mbsi.Application, _applicationName, true) == 0));

                    if (removeThisInstance == true)
                    {
                        // Look for all types of running and suspended statuses.
                        bool running = ((mbsi.InstanceStatus & InstanceStatus.RunningAll) != InstanceStatus.None);
                        bool suspended = ((mbsi.InstanceStatus & InstanceStatus.SuspendedAll) != InstanceStatus.None);

                        if (running || suspended)
                        {
                            CompletionStatus status = _operations.TerminateInstance(mbsi.ID);

                            if (status != CompletionStatus.Succeeded)
                            {
                                this.Log.LogError("Instance {0} was not successfully terminated.  Status is {1}.", mbsi.URI, status.ToString());
                                success = false;
                            }
                            else
                            {
                                terminatedServiceCount++;
                            }
                        }
                    }
                }
            }

            base.Log.LogMessage(
                MessageImportance.Normal,
                "Terminated {0} service instances for BizTalk app '{1}'.", terminatedServiceCount, removeAll ? "[ALL APPS]" : _applicationName);

            return success;
        }
    }
}
