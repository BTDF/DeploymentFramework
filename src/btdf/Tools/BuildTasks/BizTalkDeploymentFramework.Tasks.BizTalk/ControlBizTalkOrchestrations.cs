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

namespace DeploymentFramework.BuildTasks
{
    public class ControlBizTalkOrchestrations : ControlBizTalkArtifact
    {
        public ControlBizTalkOrchestrations()
            : base("Orchestration")
        {
        }

        protected override Dictionary<string, Dictionary<string, object>> GetArtifactsWithDefaultAction(Application application)
        {
            Dictionary<string, Dictionary<string, object>> artifacts = new Dictionary<string, Dictionary<string, object>>();

            foreach (BtsOrchestration btsOrch in application.Orchestrations)
            {
                Dictionary<string, object> metadata = new Dictionary<string, object>();
                metadata[ActionMetadataKey] = _defaultAction;
                artifacts[btsOrch.FullName] = metadata;
            }

            return artifacts;
        }

        protected override string[] GetKnownMetadata()
        {
            return new string[] { "DeployAction", "UndeployAction", "AutoResumeInstances" };
        }

        protected override bool ControlIndividualArtifact(string name, Dictionary<string, object> metadata, BtsCatalogExplorer catalog, Application app)
        {
            BtsOrchestration inst = app.Orchestrations[name];

            OrchestrationStatus status = inst.Status;

            ActionType action = (ActionType)metadata[ActionMetadataKey];

            switch (action)
            {
                case ActionType.Enlist:
                    if (status == OrchestrationStatus.Unenlisted)
                    {
                        this.Log.LogMessage("Enlisting orchestration '" + name + "'...");
                        inst.Status = OrchestrationStatus.Enlisted;
                        return true;
                    }
                    break;
                case ActionType.Unenlist:
                    if (status != OrchestrationStatus.Unenlisted)
                    {
                        this.Log.LogMessage("Unenlisting orchestration '" + name + "'...");
                        inst.AutoTerminateInstances = false;
                        inst.Status = OrchestrationStatus.Unenlisted;
                        return true;
                    }
                    break;
                case ActionType.Start:
                    if (status != OrchestrationStatus.Started)
                    {
                        StartOrchestration(name, metadata, catalog, inst);
                        return true;
                    }
                    break;
                case ActionType.Stop:
                    if (status == OrchestrationStatus.Started)
                    {
                        this.Log.LogMessage("Stopping orchestration '" + name + "'...");
                        // No auto disable receive locations
                        // Auto suspend running instances
                        inst.AutoSuspendRunningInstances = true;
                        inst.Status = OrchestrationStatus.Enlisted;
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void StartOrchestration(string name, Dictionary<string, object> metadata, BtsCatalogExplorer catalog, BtsOrchestration inst)
        {
            bool autoResumeSuspended = false;
            object autoResumeSuspendedStr = null;
            if (metadata.TryGetValue("AutoResumeInstances", out autoResumeSuspendedStr))
            {
                bool.TryParse((string)autoResumeSuspendedStr, out autoResumeSuspended);
            }

            this.Log.LogMessage("Starting orchestration '" + name + "'...");
            // No auto enable receive locations
            // No auto resume service instances
            // No auto start send ports
            inst.AutoResumeSuspendedInstances = autoResumeSuspended;
            inst.Status = OrchestrationStatus.Started;
        }
    }
}
