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
    /// Adds or removes an IIS web service extension.
    /// </summary>
    public class ConfigureIISWebServiceExtension : ConfigureIISTask
    {
        protected override bool Configure(ServerManager mgr, ITaskItem ti, ModeType mode, ActionType action)
        {
            string siteName = ti.GetMetadata("SiteName");
            string physicalPath = ti.GetMetadata("PhysicalPath");
            string virtualPath = ti.GetMetadata("VirtualPath");
            string isapiFileName = ti.GetMetadata("IsapiFileName");

            // If no IsapiFileName, then nothing to do
            if (string.IsNullOrWhiteSpace(isapiFileName))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(physicalPath) || string.IsNullOrWhiteSpace(virtualPath))
            {
                base.Log.LogError("Required metadata values PhysicalPath or VirtualPath are missing.");
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

            physicalPath = Path.Combine(physicalPath, isapiFileName);

            if (!ConfigureIsapiRestrictions(mgr, physicalPath, mode))
            {
                return false;
            }

            if (!ConfigureHandler(mgr, siteName, virtualPath, physicalPath, isapiFileName, mode))
            {
                return false;
            }

            return true;
        }

        private bool ConfigureIsapiRestrictions(ServerManager mgr, string physicalPath, ModeType mode)
        {
            Configuration config = mgr.GetApplicationHostConfiguration();

            ConfigurationSection isapiCgiRestrictionSection = config.GetSection("system.webServer/security/isapiCgiRestriction");
            ConfigurationElementCollection isapiCgiRestrictionCollection = isapiCgiRestrictionSection.GetCollection();

            ConfigurationElement addElement =
                isapiCgiRestrictionCollection.FirstOrDefault(elem => string.Compare(elem.Attributes["path"].Value.ToString(), physicalPath, true) == 0);

            if (mode == ModeType.Deploy && addElement == null)
            {
                base.Log.LogMessage("Allowing ISAPI restriction '{0}'...", physicalPath);
                addElement = isapiCgiRestrictionCollection.CreateElement("add");
                addElement["path"] = physicalPath;
                addElement["allowed"] = true;
                isapiCgiRestrictionCollection.Add(addElement);
                mgr.CommitChanges();
                base.Log.LogMessage("Allowed ISAPI restriction '{0}'.", physicalPath);
            }
            else if (mode == ModeType.Undeploy && addElement != null)
            {
                base.Log.LogMessage("Disallowing ISAPI restriction '{0}'...", physicalPath);
                isapiCgiRestrictionCollection.Remove(addElement);
                mgr.CommitChanges();
                base.Log.LogMessage("Disallowed ISAPI restriction '{0}'...", physicalPath);
            }

            return true;
        }

        private bool ConfigureHandler(ServerManager mgr, string siteName, string virtualPath, string physicalPath, string isapiFileName, ModeType mode)
        {
            Site ste = mgr.Sites[siteName];

            if (ste == null)
            {
                if (mode == ModeType.Undeploy)
                {
                    base.Log.LogWarning("Cannot find IIS site '" + siteName + "'.");
                    return true;
                }
                else
                {
                    base.Log.LogError("Cannot find IIS site '" + siteName + "'.");
                    return false;
                }
            }

            Application app = ste.Applications[virtualPath];

            if (app == null)
            {
                if (mode == ModeType.Undeploy)
                {
                    base.Log.LogWarning("Cannot find IIS application '" + virtualPath + "'.");
                    return true;
                }
                else
                {
                    base.Log.LogError("Cannot find IIS application '" + virtualPath + "'.");
                    return false;
                }
            }

            Configuration config = app.GetWebConfiguration();

            ConfigurationSection handlersSection = config.GetSection("system.webServer/handlers");
            ConfigurationElementCollection handlersCollection = handlersSection.GetCollection();

            ConfigurationElement addElement =
                handlersCollection.FirstOrDefault(elem => string.Compare(elem.Attributes["scriptProcessor"].Value.ToString(), physicalPath, true) == 0);

            if (mode == ModeType.Deploy)
            {
                base.Log.LogMessage("Adding handler mapping '{0}'...", physicalPath);
                if (addElement == null)
                {
                    addElement = handlersCollection.CreateElement("add");
                    addElement["name"] = isapiFileName;
                    addElement["path"] = isapiFileName;
                    addElement["verb"] = "*";
                    addElement["modules"] = "IsapiModule";
                    addElement["scriptProcessor"] = physicalPath;
                    handlersCollection.Clear();
                    handlersCollection.AddAt(0, addElement);
                }
                else
                {
                    addElement["name"] = isapiFileName;
                    addElement["path"] = isapiFileName;
                    addElement["verb"] = "*";
                    addElement["modules"] = "IsapiModule";
                    addElement["scriptProcessor"] = physicalPath;
                }
                mgr.CommitChanges();
                base.Log.LogMessage("Added handler mapping '{0}'.", physicalPath);
            }
            else if (mode == ModeType.Undeploy && addElement != null)
            {
                base.Log.LogMessage("Removing handler mapping '{0}'...", physicalPath);
                handlersCollection.Remove(addElement);
                mgr.CommitChanges();
                base.Log.LogMessage("Removed handler mapping '{0}'...", physicalPath);
            }

            return true;
        }
    }
}
