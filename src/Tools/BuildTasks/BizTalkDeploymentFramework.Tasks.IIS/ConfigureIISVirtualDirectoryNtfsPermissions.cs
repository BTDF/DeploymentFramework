// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Web.Administration;
using System.Security.AccessControl;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Grants NTFS permissions on folders used by IIS applications
    /// </summary>
    public class ConfigureIISVirtualDirectoryNtfsPermissions : ConfigureIISTask
    {
        protected override bool Configure(ServerManager mgr, ITaskItem ti, ModeType mode, ActionType action)
        {
            if (mode == ModeType.Deploy)
            {
                return Deploy(mgr, ti);
            }

            return true;
        }

        private bool Deploy(ServerManager mgr, ITaskItem ti)
        {
            string appPoolName = ti.GetMetadata("AppPoolName");
            string physicalPath = ti.GetMetadata("PhysicalPath");

            if (string.IsNullOrWhiteSpace(physicalPath) || string.IsNullOrWhiteSpace(appPoolName))
            {
                base.Log.LogError("Required metadata values PhysicalPath or AppPoolName are missing.");
                return false;
            }

            // Convert a relative path to an absolute path
            if (!Path.IsPathRooted(physicalPath))
            {
                string currentDirectory = Environment.CurrentDirectory;
                Environment.CurrentDirectory = this.MSBuildProjectDirectory;
                physicalPath = new DirectoryInfo(physicalPath).FullName;
                Environment.CurrentDirectory = currentDirectory;
            }

            ApplicationPool appPool = mgr.ApplicationPools[appPoolName];

            if (appPool == null)
            {
                base.Log.LogError("Cannot find IIS application pool '" + appPoolName + "'.");
                return false;
            }

            string userName = null;

            switch (appPool.ProcessModel.IdentityType)
            {
                case ProcessModelIdentityType.LocalService:
                    userName = "LocalService";
                    break;
                case ProcessModelIdentityType.LocalSystem:
                    userName = "LocalSystem";
                    break;
                case ProcessModelIdentityType.NetworkService:
                    userName = "NetworkService";
                    break;
                case ProcessModelIdentityType.SpecificUser:
                    userName = appPool.ProcessModel.UserName;
                    userName = userName.Replace(".\\", string.Empty);
                    break;
                case ProcessModelIdentityType.ApplicationPoolIdentity:
                    userName = @"IIS AppPool\" + appPoolName;
                    break;
                default:
                    base.Log.LogError("Invalid value in IdentityType metadata element");
                    return false;
            }

            base.Log.LogMessage("Granting NTFS permissions on '" + physicalPath + "' to '" + userName + "'...");
            SetDirectorySecurity(physicalPath, userName, FileSystemRights.ReadAndExecute | FileSystemRights.ListDirectory, AccessControlType.Allow);
            base.Log.LogMessage("Granted NTFS permissions on '" + physicalPath + "'.");

            return true;
        }

        private static void SetDirectorySecurity(string path, string userName, FileSystemRights rights, AccessControlType controlType)
        {
            // Sets an ACL entry on the specified directory for the specified account.

            // Get a DirectorySecurity object that represents the current security settings.
            DirectorySecurity dSecurity = Directory.GetAccessControl(path);

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.AddAccessRule(
                new FileSystemAccessRule(
                    userName, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, controlType));

            // Set the new access settings.
            Directory.SetAccessControl(path, dSecurity);
        }
    }
}
