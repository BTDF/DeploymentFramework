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
    public class ResumeServiceInstances : Task
    {
        private string _applicationName;

        public ResumeServiceInstances()
        {
        }

        /// <summary>
        /// The application for which to resume suspended instances.  "*" will resume for all applications.
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

            int resumedServiceCount = 0;
            bool success = true;
            bool resumeAll = (_applicationName.Trim() == "*");

            base.Log.LogMessage(
                MessageImportance.Normal,
                "Attempting to resume all service instances for BizTalk app '{0}'...", resumeAll ? "[ALL APPS]" : _applicationName);

            using (BizTalkOperations operations = new BizTalkOperations())
            {
                // Get all service instances in the message box.
                // This is terribly inefficient -- BizTalkOperations has a way to directly query and filter by app, but the classes are Internal.
                // Could use reflection to call it anyway, but risks compatibility across BizTalk releases.
                foreach (Instance instance in operations.GetServiceInstances())
                {
                    MessageBoxServiceInstance mbsi = instance as MessageBoxServiceInstance;

                    if (mbsi != null)
                    {
                        // Only resume if the application matches the one we are interested in.  "*" will match all apps.
                        bool resumeThisInstance = (resumeAll || (string.Compare(mbsi.Application, _applicationName, true) == 0));
                        bool suspended = ((mbsi.InstanceStatus & InstanceStatus.SuspendedAll) != InstanceStatus.None);

                        if (resumeThisInstance && suspended)
                        {
                            CompletionStatus status = operations.ResumeInstance(mbsi.ID);

                            if (status != CompletionStatus.Succeeded)
                            {
                                this.Log.LogWarning("Could not resume instance {0}.  Status is {1}.", mbsi.ID, status.ToString());
                                success = false;
                            }
                            else
                            {
                                resumedServiceCount++;
                            }

                            if (resumedServiceCount % 5000 == 0)
                            {
                                base.Log.LogMessage(
                                    MessageImportance.Normal,
                                    "Resumed {0} service instances for BizTalk app '{1}'.", resumedServiceCount, resumeAll ? "[ALL APPS]" : _applicationName);
                            }
                        }
                    }
                }
            }

            base.Log.LogMessage(
                MessageImportance.Normal,
                "Finished resuming {0} service instances for BizTalk app '{1}'.", resumedServiceCount, resumeAll ? "[ALL APPS]" : _applicationName);

            return success;
        }
    }
}
