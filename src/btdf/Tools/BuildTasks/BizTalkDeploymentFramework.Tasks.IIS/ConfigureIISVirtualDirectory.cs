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

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Configures one or more IIS virtual directories and applications.
    /// </summary>
    public class ConfigureIISVirtualDirectory : ConfigureIISTask
    {
        protected override bool Configure(ServerManager mgr, ITaskItem ti, ModeType mode, ActionType action)
        {
            string siteName = ti.GetMetadata("SiteName");
            string virtualPath = ti.GetMetadata("VirtualPath");

            if (string.IsNullOrWhiteSpace(siteName) || string.IsNullOrWhiteSpace(virtualPath))
            {
                base.Log.LogError("Required metadata values SiteName or VirtualPath are missing.");
                return false;
            }

            virtualPath = virtualPath.Trim();

            if (!virtualPath.StartsWith("/"))
            {
                virtualPath = "/" + virtualPath;
            }

            if (virtualPath.EndsWith("/"))
            {
                virtualPath = virtualPath.TrimEnd('/');
            }

            if (mode == ModeType.Deploy)
            {
                return Deploy(mgr, ti, mode, action, siteName, virtualPath);
            }
            else if (mode == ModeType.Undeploy)
            {
                return Undeploy(mgr, action, siteName, virtualPath);
            }

            return true;
        }

        private bool Deploy(ServerManager mgr, ITaskItem ti, ModeType mode, ActionType action, string siteName, string virtualPath)
        {
            string physicalPath = ti.GetMetadata("PhysicalPath");
            string appPoolName = ti.GetMetadata("AppPoolName");
            string enabledProtocols = ti.GetMetadata("EnabledProtocols");
            string deployActionStr = ti.GetMetadata("DeployAction");

            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                base.Log.LogError("Required metadata value PhysicalPath is missing.");
                return false;
            }

            if (physicalPath.EndsWith("\\"))
            {
                physicalPath = physicalPath.TrimEnd('\\');
            }

            // Convert a relative path to an absolute path
            if (!Path.IsPathRooted(physicalPath))
            {
                string currentDirectory = Environment.CurrentDirectory;
                Environment.CurrentDirectory = this.MSBuildProjectDirectory;
                physicalPath = new DirectoryInfo(physicalPath).FullName;
                Environment.CurrentDirectory = currentDirectory;
            }

            return DeployApplication(mgr, action, siteName, virtualPath, physicalPath, appPoolName, enabledProtocols);
        }

        private bool Undeploy(ServerManager mgr, ActionType action, string siteName, string virtualPath)
        {
            if (action != ActionType.Delete)
            {
                return true;
            }

            Site ste = mgr.Sites[siteName];

            if (ste == null)
            {
                base.Log.LogWarning("Cannot find IIS site '" + siteName + "'.");
                return true;
            }

            List<string> virtualPaths = new List<string>();

            GenerateAllPaths(virtualPath, null, virtualPaths, null);

            // We assume that it's safe to delete the app located at the complete user-specified virtual path
            Application app = ste.Applications[virtualPaths[0]];

            if (app != null)
            {
                ste.Applications.Remove(app);
            }

            // Now we need to walk up the hierarchy toward the root application, cleaning up whatever we can.
            // If other applications or virtual directories are present at some level that we didn't create,
            // then abort the delete and leave the rest intact.
            for (int index = 1; index < virtualPaths.Count - 1; index++)
            {
                int closestAppVirtualPathIndex2;

                Application closestApp = FindClosestApplication(ste.Applications, virtualPaths, out closestAppVirtualPathIndex2);

                string vdirPath = virtualPaths[index].Remove(0, closestApp.Path.Length);

                if (!vdirPath.StartsWith("/"))
                {
                    vdirPath = "/" + vdirPath;
                }

                List<VirtualDirectory> vdirs = closestApp.VirtualDirectories.Where(x => x.Path.StartsWith(vdirPath)).ToList();
                List<Application> apps = ste.Applications.Where(x => x.Path.StartsWith(virtualPaths[index])).ToList();

                // If we found more than one virtual directory (our own) at or below this node, or an application other than one
                // at the current node (our own), then abort the delete.
                if (vdirs.Count > 1 || (apps.Count > 0 && apps[0].Path != virtualPaths[index]))
                {
                    break;
                }
                else if (apps.Count == 1)
                {
                    ste.Applications.Remove(apps[0]);
                }
                else if (vdirs.Count == 1 && vdirPath != "/")
                {
                    closestApp.VirtualDirectories.Remove(vdirs[0]);
                }
            }

            base.Log.LogMessage("Removing IIS application '" + virtualPath + "'...");
            mgr.CommitChanges();
            base.Log.LogMessage("Removed IIS application '" + virtualPath + "'.");

            return true;
        }

        private bool DeployApplication(
            ServerManager mgr, ActionType action, string siteName, string virtualPath, string physicalPath, string appPoolName, string enabledProtocols)
        {
            Site ste = mgr.Sites[siteName];

            if (ste == null)
            {
                base.Log.LogError("Cannot find IIS site '" + siteName + "'.");
                return false;
            }

            Application app = ste.Applications[virtualPath];

            if (action == ActionType.CreateOrUpdate)
            {
                if (app != null)
                {
                    base.Log.LogMessage("Updating IIS application '" + virtualPath + "'...");
                }
                else
                {
                    base.Log.LogMessage("Creating IIS application '" + virtualPath + "'...");
                    if ((app = CreateApplication(ste, virtualPath, physicalPath)) == null)
                    {
                        return false;
                    }
                }
            }
            else if (action == ActionType.Create)
            {
                if (app != null)
                {
                    base.Log.LogWarning("DeployAction is set to Create but the IIS application '" + virtualPath + "' already exists. Skipping.");
                    return true;
                }

                base.Log.LogMessage("Creating IIS application '" + virtualPath + "'...");
                if ((app = CreateApplication(ste, virtualPath, physicalPath)) == null)
                {
                    return false;
                }
            }
            else if (action == ActionType.Update)
            {
                if (app == null)
                {
                    base.Log.LogError("DeployAction is set to Update but the IIS application '" + virtualPath + "' does not exist.");
                    return false;
                }

                base.Log.LogMessage("Updating IIS application '" + virtualPath + "'...");
            }

            if (!string.IsNullOrWhiteSpace(appPoolName))
            {
                app.ApplicationPoolName = appPoolName;
            }

            if (!string.IsNullOrWhiteSpace(enabledProtocols))
            {
                app.EnabledProtocols = enabledProtocols;
            }

            mgr.CommitChanges();
            base.Log.LogMessage("Created/updated IIS application '" + virtualPath + "'.");

            return true;
        }

        /// <summary>
        /// Create an IIS application
        /// </summary>
        /// <param name="ste">IIS Site object</param>
        /// <param name="virtualPath">Virtual path of new application</param>
        /// <param name="physicalPath">Physical path of new application</param>
        /// <returns>Newly created application</returns>
        /// <remarks>
        /// This method assumes that an application does not currently exist at the virtualPath location
        /// It also assumes that the physical path contains at least as many path segments as the virtual path.
        /// For example, if the virtual path is /My/App the physical path must also have at least two segments,
        /// like ..\My\App or C:\My\App, but ..\Its\My\App is also OK because it has more than the minimum.
        /// </remarks>
        private Application CreateApplication(Site ste, string virtualPath, string physicalPath)
        {
            bool isChildOfRoot = (virtualPath.LastIndexOf('/') == 0);

            // If the virtual path is an immediate child of the root, we can directly add the app.
            if (isChildOfRoot)
            {
                return ste.Applications.Add(virtualPath, physicalPath);
            }

            // We need extra complexity to handle cases where the virtualPath is not an immediate child of the root.
            // We have to make sure that all levels of the virtualPath tree exist in IIS as virtual directories and/or apps.

            List<string> virtualPaths = new List<string>();
            List<string> physicalPaths = new List<string>();

            GenerateAllPaths(virtualPath, physicalPath, virtualPaths, physicalPaths);

            int closestAppVirtualPathIndex;

            Application closestApp = FindClosestApplication(ste.Applications, virtualPaths, out closestAppVirtualPathIndex);

            for (int vdirIndex = closestAppVirtualPathIndex - 1; vdirIndex > 0; vdirIndex--)
            {
                string vdirPath = virtualPaths[vdirIndex].Remove(0, closestApp.Path.Length);

                if (!vdirPath.StartsWith("/"))
                {
                    vdirPath = "/" + vdirPath;
                }

                VirtualDirectory vdir = closestApp.VirtualDirectories[vdirPath];

                if (vdir == null)
                {
                    vdir = closestApp.VirtualDirectories.CreateElement();
                    vdir.PhysicalPath = physicalPaths[vdirIndex];
                    vdir.Path = virtualPaths[vdirIndex];
                    closestApp.VirtualDirectories.Add(vdir);
                }
            }

            return ste.Applications.Add(virtualPaths[0], physicalPaths[0]);
        }

        private void GenerateAllPaths(string virtualPath, string physicalPath, List<string> virtualPaths, List<string> physicalPaths)
        {
            string workingVirtualPath = virtualPath + "/";
            string workingPhysicalPath = physicalPath == null ? null : physicalPath + @"\";

            while (workingVirtualPath.Length > 0)
            {
                workingVirtualPath = workingVirtualPath.Remove(workingVirtualPath.LastIndexOf("/"));
                if (workingVirtualPath.Length == 0)
                {
                    virtualPaths.Add("/");
                }
                else
                {
                    virtualPaths.Add(workingVirtualPath);
                }

                if (physicalPaths != null)
                {
                    workingPhysicalPath = workingPhysicalPath.Remove(workingPhysicalPath.LastIndexOf(@"\"));
                    physicalPaths.Add(workingPhysicalPath);
                }
            }
        }

        private Application FindClosestApplication(ApplicationCollection apps, List<string> virtualPaths, out int closestAppVirtualPathIndex)
        {
            // This function assumes that the virtualPaths list always contains "/", the default and always-present application

            closestAppVirtualPathIndex = -1;

            foreach (string virtualPath in virtualPaths)
            {
                closestAppVirtualPathIndex += 1;

                Application closestApp = apps[virtualPath];

                if (closestApp != null)
                {
                    return closestApp;
                }
            }

            throw new Exception("Cannot find any valid IIS application.");
        }
    }
}
