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
    public class ControlBizTalkSendPortGroups : ControlBizTalkArtifact
    {
        public ControlBizTalkSendPortGroups()
            : base("Send Port Group")
        {
        }

        protected override Dictionary<string, Dictionary<string, object>> GetArtifactsWithDefaultAction(Application application)
        {
            Dictionary<string, Dictionary<string, object>> artifacts = new Dictionary<string, Dictionary<string, object>>();

            foreach (SendPortGroup spg in application.SendPortGroups)
            {
                Dictionary<string, object> metadata = new Dictionary<string, object>();
                metadata[ActionMetadataKey] = _defaultAction;
                artifacts[spg.Name] = metadata;
            }

            return artifacts;
        }

        protected override string[] GetKnownMetadata()
        {
            return new string[] { "DeployAction", "UndeployAction" };
        }

        protected override bool ControlIndividualArtifact(string name, Dictionary<string, object> metadata, BtsCatalogExplorer catalog, Application app)
        {
            SendPortGroup inst = app.SendPortGroups[name];

            PortStatus status = inst.Status;

            ActionType action = (ActionType)metadata[ActionMetadataKey];

            switch (action)
            {
                case ActionType.Enlist:
                    if (status == PortStatus.Bound)
                    {
                        this.Log.LogMessage(action.ToString() + "ing send port group '" + name + "'...");
                        inst.Status = PortStatus.Stopped;
                        return true;
                    }
                    break;
                case ActionType.Unenlist:
                    if (status != PortStatus.Bound)
                    {
                        this.Log.LogMessage("Unenlisting send port group '" + name + "'...");
                        inst.Status = PortStatus.Bound;
                        return true;
                    }
                    break;
                case ActionType.Start:
                    if (status != PortStatus.Started)
                    {
                        this.Log.LogMessage("Starting send port group '" + name + "'...");
                        inst.Status = PortStatus.Started;
                        return true;
                    }
                    break;
                case ActionType.Stop:
                    if (status == PortStatus.Started)
                    {
                        this.Log.LogMessage(action.ToString() + "ing send port group '" + name + "'...");
                        inst.Status = PortStatus.Stopped;
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
