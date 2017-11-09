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
    public class ControlBizTalkReceiveLocations : ControlBizTalkArtifact
    {
        public ControlBizTalkReceiveLocations()
            : base("Receive Location")
        {
        }

        protected override Dictionary<string, Dictionary<string, object>> GetArtifactsWithDefaultAction(Application application)
        {
            Dictionary<string, Dictionary<string, object>> artifacts = new Dictionary<string, Dictionary<string, object>>();

            foreach (ReceivePort rp in application.ReceivePorts)
            {
                foreach (ReceiveLocation rl in rp.ReceiveLocations)
                {
                    Dictionary<string, object> metadata = new Dictionary<string, object>();
                    metadata[ActionMetadataKey] = _defaultAction;
                    artifacts[rl.Name] = metadata;
                }
            }

            return artifacts;
        }

        protected override string[] GetKnownMetadata()
        {
            return new string[] { "DeployAction", "UndeployAction" };
        }

        protected override bool ControlIndividualArtifact(string name, Dictionary<string, object> metadata, BtsCatalogExplorer catalog, Application app)
        {
            ReceiveLocation inst = null;

            foreach (ReceivePort rp in app.ReceivePorts)
            {
                inst = rp.ReceiveLocations[name];

                if (inst != null)
                {
                    break;
                }
            }

            ActionType action = (ActionType)metadata[ActionMetadataKey];

            bool actionEnable = (action == ActionType.Enable);
            string preActionString = actionEnable ? "Enabling" : "Disabling";
            string postActionString = actionEnable ? "Enabled" : "Disabled";

            if (inst.Enable != actionEnable)
            {
                this.Log.LogMessage(preActionString + " receive location '" + name + "'...");
                inst.Enable = actionEnable;
                return true;
            }

            return false;
        }
    }
}
