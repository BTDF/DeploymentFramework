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
    public class CheckForServiceInstances : Task
    {
        private string _applicationName;

        public CheckForServiceInstances()
        {
        }

        /// <summary>
        /// The application to check for running or suspended instances for.  
        /// </summary>
        [Required]
        public string Application
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        /// <summary>
        /// This function will check to see if there are any service instances for the given
        /// BizTalk application.  If so, it will throw an exception.  
        /// </summary>
        public override bool Execute()
        {
            this.Log.LogMessage("Checking for existing service instances associated with application '{0}'...", _applicationName);

            using (BizTalkOperations _operations = new BizTalkOperations())
            {
                // Get all service instances in the message box.
                foreach (Instance instance in _operations.GetServiceInstances())
                {
                    // We will only deal with instances that we can determine 
                    // which application it belongs to.
                    MessageBoxServiceInstance mbsi = instance as MessageBoxServiceInstance;
                    if (mbsi != null)
                    {
                        if (mbsi.Application == Application)
                        {
                            this.Log.LogError(
                                "There is at least one service instance associated with the '{0}' application [Instance Status = {1}]. An application be removed only when there are no associated service instances.",
                                _applicationName,
                                Enum.GetName(typeof(InstanceStatus), mbsi.InstanceStatus));
                            return false;
                        }
                    }
                }
            }

            this.Log.LogMessage("Done checking for existing service instances associated with application '{0}'.", _applicationName);

            return true;
        }
    }
}
