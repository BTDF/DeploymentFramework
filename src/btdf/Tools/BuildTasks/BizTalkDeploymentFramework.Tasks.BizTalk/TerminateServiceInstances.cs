// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
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
        private string _applicationName;

        public TerminateServiceInstances()
        {
        }

        /// <summary>
        /// The application for which to remove running or suspended instances.  "*" will remove for all applications.
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

            const int MaxRetries = 3;

            bool success = true;
            int terminatedServiceCount = 0;
            bool removeAll = (_applicationName.Trim() == "*");

            base.Log.LogMessage(
                MessageImportance.Normal,
                "Attempting to terminate all service instances for BizTalk app '{0}'...", removeAll ? "[ALL APPS]" : _applicationName);

            for (int retryCount = 0; retryCount < MaxRetries; retryCount++)
            {
                success = true;

                // Create a new BizTalkOperations instance each time to avoid potential caching issues
                using (BizTalkOperations operations = new BizTalkOperations())
                {
                    // Get all service instances in the message box.
                    // This is terribly inefficient -- BizTalkOperations has a way to directly query and filter by app, but the classes are Internal.
                    // Could use reflection to call it anyway, but risks compatibility across BizTalk releases.
                    foreach (Instance instance in operations.GetServiceInstances())
                    {
                        MessageBoxServiceInstance mbsi = instance as MessageBoxServiceInstance;

                        if (mbsi == null)
                        {
                            continue;
                        }

                        // Only terminate if the application matches the one we are interested in.  "*" will match all apps.
                        bool removeThisInstance = (removeAll || (string.Compare(mbsi.Application, _applicationName, true) == 0));
                        bool running = ((mbsi.InstanceStatus & InstanceStatus.RunningAll) != InstanceStatus.None);
                        bool suspended = ((mbsi.InstanceStatus & InstanceStatus.SuspendedAll) != InstanceStatus.None);

                        if (removeThisInstance && (running || suspended))
                        {
                            CompletionStatus status = operations.TerminateInstance(mbsi.ID);

                            if (status != CompletionStatus.Succeeded)
                            {
                                if (instance.Class == ServiceClass.RoutingFailure)
                                {
                                    this.Log.LogMessage(MessageImportance.Low, "Could not terminate routing failure {0}.  It was probably terminated by a linked instance.", mbsi.ID);
                                }
                                else
                                {
                                    if (retryCount == MaxRetries - 1)
                                    {
                                        this.Log.LogError("Could not terminate instance {0}.  Status is {1}.", mbsi.ID, status.ToString());
                                    }
                                    success = false;
                                }
                            }
                            else
                            {
                                terminatedServiceCount++;
                            }
                        }
                    }
                }

                if (success)
                {
                    break;
                }
            }

            base.Log.LogMessage(
                MessageImportance.Normal,
                "Terminated {0} service instances for BizTalk app '{1}'.", terminatedServiceCount, removeAll ? "[ALL APPS]" : _applicationName);

            return success;
        }
    }
}
