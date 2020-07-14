// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.BizTalk.ExplorerOM;
using System.Collections.Specialized;

namespace DeploymentFramework.BuildTasks
{
    public class ControlBizTalkReferencedApps : ControlBizTalkArtifact
    {
        public ControlBizTalkReferencedApps()
            : base("Referenced Application")
        {
        }

        protected override Dictionary<string, Dictionary<string, object>> GetArtifactsWithDefaultAction(Application application)
        {
            Dictionary<string, Dictionary<string, object>> artifacts = new Dictionary<string, Dictionary<string, object>>();

            StringCollection referencedAppNames = new StringCollection();
            GetAllReferencedAppsRecursive(application, referencedAppNames);
            referencedAppNames.Remove(application.Name);

            foreach (string appName in referencedAppNames)
            {
                Dictionary<string, object> metadata = new Dictionary<string, object>();
                metadata[ActionMetadataKey] = _defaultAction;
                artifacts[appName] = metadata;
            }

            return artifacts;
        }

        protected override string[] GetKnownMetadata()
        {
            return new string[] { "DeployAction", "ApplicationStartOptions" };
        }

        protected override bool ControlIndividualArtifact(string name, Dictionary<string, object> metadata, BtsCatalogExplorer catalog, Application app)
        {
            Application inst = catalog.Applications[name];

            StartApplication(name, metadata, catalog, inst);

            return false;  // Don't want the base class to SaveChanges
        }

        private void StartApplication(string name, Dictionary<string, object> metadata, BtsCatalogExplorer catalog, Application inst)
        {
            ApplicationStartOption startOption =
                ApplicationStartOption.StartAllSendPorts | ApplicationStartOption.StartAllSendPortGroups | ApplicationStartOption.StartAllOrchestrations | ApplicationStartOption.EnableAllReceiveLocations | ApplicationStartOption.DeployAllPolicies;
            object startOptionStr = null;
            if (metadata.TryGetValue("ApplicationStartOptions", out startOptionStr))
            {
                startOption = ControlBizTalkApp.ParseStartEnum((string)startOptionStr);
            }

            this.Log.LogMessage("Starting referenced application '" + name + "'...");
            inst.Start(startOption);
            catalog.SaveChanges();
            this.Log.LogMessage("Started referenced application '" + name + "'.");
        }

        private void GetAllReferencedAppsRecursive(Application application, StringCollection referencedAppNames)
        {
            foreach (Application app in application.References)
            {
                GetAllReferencedAppsRecursive(app, referencedAppNames);
            }

            if (!referencedAppNames.Contains(application.Name) && string.Compare(application.Name, "BizTalk.System", true) != 0)
            {
                referencedAppNames.Add(application.Name);
            }
        }
    }
}
