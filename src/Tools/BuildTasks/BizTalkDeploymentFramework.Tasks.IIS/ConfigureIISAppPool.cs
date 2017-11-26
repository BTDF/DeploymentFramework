// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Web.Administration;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Configures one or more IIS application pools.
    /// </summary>
    public class ConfigureIISAppPool : ConfigureIISTask
    {
        protected override bool Configure(ServerManager mgr, ITaskItem ti, ModeType mode, ActionType action)
        {
            string appPoolName = ti.ItemSpec;

            if (mode == ModeType.Deploy)
            {
                return Deploy(mgr, ti, mode, action, appPoolName);
            }
            else if (mode == ModeType.Undeploy)
            {
                return Undeploy(mgr, action, appPoolName);
            }

            return true;
        }

        private bool Deploy(ServerManager mgr, ITaskItem ti, ModeType mode, ActionType action, string appPoolName)
        {
            string frameworkVersion = ti.GetMetadata("DotNetFrameworkVersion");
            string pipelineModeStr = ti.GetMetadata("PipelineMode");
            string enable32BitStr = ti.GetMetadata("Enable32Bit");
            string identityTypeStr = ti.GetMetadata("IdentityType");
            string userName = ti.GetMetadata("UserName");
            string password = ti.GetMetadata("Password");

            bool enable32Bit;
            if (!bool.TryParse(enable32BitStr, out enable32Bit))
            {
                base.Log.LogError("Invalid Enable32Bit metadata value " + enable32BitStr);
                return false;
            }

            ManagedPipelineMode pipelineMode = ManagedPipelineMode.Integrated;
            if (!Enum.TryParse<ManagedPipelineMode>(pipelineModeStr, true, out pipelineMode))
            {
                base.Log.LogError("Invalid PipelineMode metadata value " + pipelineModeStr);
                return false;
            }

            ProcessModelIdentityType identityType = ProcessModelIdentityType.ApplicationPoolIdentity;
            if (!Enum.TryParse<ProcessModelIdentityType>(identityTypeStr, true, out identityType))
            {
                base.Log.LogError("Invalid IdentityType metadata value " + identityTypeStr);
                return false;
            }

            if (identityType == ProcessModelIdentityType.SpecificUser && (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)))
            {
                base.Log.LogError("IdentityType is set to SpecificUser but either the UserName or the Password is missing.");
                return false;
            }

            return DeployApplicationPool(mgr, action, appPoolName, identityType, userName, password, frameworkVersion, true, enable32Bit, pipelineMode);
        }

        private bool DeployApplicationPool(
            ServerManager mgr, ActionType action, string appPoolName, ProcessModelIdentityType identityType, string userName, string password,
            string managedRuntimeVersion, bool autoStart, bool enable32BitAppOnWin64, ManagedPipelineMode managedPipelineMode)
        {
            ApplicationPool appPool = mgr.ApplicationPools[appPoolName];

            if (action == ActionType.CreateOrUpdate)
            {
                if (appPool != null)
                {
                    base.Log.LogMessage("Updating IIS application pool '" + appPoolName + "'...");
                }
                else
                {
                    base.Log.LogMessage("Creating IIS application pool '" + appPoolName + "'...");
                    appPool = mgr.ApplicationPools.Add(appPoolName);
                }
            }
            else if (action == ActionType.Create)
            {
                if (appPool != null)
                {
                    base.Log.LogWarning("DeployAction is set to Create but the IIS application pool '" + appPoolName + "' already exists. Skipping.");
                    return true;
                }

                base.Log.LogMessage("Creating IIS application pool '" + appPoolName + "'...");
                appPool = mgr.ApplicationPools.Add(appPoolName);
            }
            else if (action == ActionType.Update)
            {
                if (appPool == null)
                {
                    base.Log.LogError("DeployAction is set to Update but the IIS application pool '" + appPoolName + "' does not exist.");
                    return false;
                }

                base.Log.LogMessage("Updating IIS application pool '" + appPoolName + "'...");
            }

            if (identityType == ProcessModelIdentityType.SpecificUser)
            {
                appPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
                appPool.ProcessModel.UserName = userName;
                appPool.ProcessModel.Password = password;
            }
            else
            {
                appPool.ProcessModel.IdentityType = identityType;
            }

            appPool.ManagedRuntimeVersion = managedRuntimeVersion;
            appPool.AutoStart = autoStart;
            appPool.Enable32BitAppOnWin64 = enable32BitAppOnWin64;
            appPool.ManagedPipelineMode = managedPipelineMode;

            mgr.CommitChanges();
            base.Log.LogMessage("Created/updated IIS application pool '" + appPoolName + "'.");

            return true;
        }

        private bool Undeploy(ServerManager mgr, ActionType action, string appPoolName)
        {
            if (action != ActionType.Delete)
            {
                return true;
            }

            ApplicationPool appPool = mgr.ApplicationPools[appPoolName];

            if (appPool == null)
            {
                return true;
            }

            base.Log.LogMessage("Removing IIS application pool '" + appPoolName + "'...");
            mgr.ApplicationPools.Remove(appPool);
            mgr.CommitChanges();
            base.Log.LogMessage("Removed IIS application pool '" + appPoolName + "'.");

            return true;
        }
    }
}
