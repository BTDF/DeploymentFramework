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
    public abstract class ControlBizTalkArtifact : Task
    {
        protected enum ModeType
        {
            Deploy,
            Undeploy
        }

        protected enum ActionType
        {
            Enlist,
            Unenlist,
            Start,
            Stop,
            Enable,
            Disable
        }

        protected const string ActionMetadataKey = "Action";

        protected ModeType _mode;
        protected ActionType _defaultAction;
        protected string _applicationName;
        protected ITaskItem[] _artifacts;
        protected string _artifactName = "Orchestration";

        [Required]
        public string Mode
        {
            get { return _mode.ToString(); }
            set { _mode = GetMode(value); }
        }

        [Required]
        public string DefaultAction
        {
            get { return _defaultAction.ToString(); }
            set { _defaultAction = GetAction(value); }
        }

        [Required]
        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public ITaskItem[] Artifacts
        {
            get { return _artifacts; }
            set { _artifacts = value; }
        }

        public ControlBizTalkArtifact(string artifactName)
        {
            _artifactName = artifactName;
        }

        public override bool Execute()
        {
            try
            {
                using (BtsCatalogExplorer catalog = BizTalkCatalogExplorerFactory.GetCatalogExplorer())
                {
                    Application application = catalog.Applications[_applicationName];
                    if (application == null)
                    {
                        this.Log.LogError("Unable to find application '{0}' in catalog.", _applicationName);
                        return false;
                    }

                    // First, get all of the artifacts associated with the BizTalk application
                    Dictionary<string, Dictionary<string, object>> artifacts = GetArtifactsWithDefaultAction(application);

                    if (artifacts == null)
                    {
                        return false;
                    }

                    // Next, reconcile the user-specified artifacts with the actual artifacts
                    if (_artifacts != null)
                    {
                        foreach (ITaskItem ti in _artifacts)
                        {
                            if (!artifacts.ContainsKey(ti.ItemSpec))
                            {
                                base.Log.LogWarning(_artifactName + " '" + ti.ItemSpec + "' does not exist in the BizTalk application.");
                                continue;
                            }

                            Dictionary<string, object> metadata = artifacts[ti.ItemSpec] as Dictionary<string, object>;

                            CopyMetadata(ti, metadata);

                            string actionMetadataName = (_mode == ModeType.Deploy) ? "DeployAction" : "UndeployAction";
                            object actionMetadataValue = null;

                            if (metadata.TryGetValue(actionMetadataName, out actionMetadataValue))
                            {
                                metadata[ActionMetadataKey] = GetAction((string)actionMetadataValue);
                            }
                        }
                    }

                    // Finally, carry out the actions against the artifacts
                    ControlArtifacts(catalog, application, artifacts);
                }

                return true;
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, false);
                return false;
            }
        }

        private void ControlArtifacts(BtsCatalogExplorer catalog, Application application, Dictionary<string, Dictionary<string, object>> artifacts)
        {
            bool needSave = false;

            foreach (KeyValuePair<string, Dictionary<string, object>> kvp in artifacts)
            {
                try
                {
                    if (ControlIndividualArtifact(kvp.Key, kvp.Value, catalog, application))
                    {
                        needSave = true;
                    }
                }
                catch (Exception)
                {
                    catalog.DiscardChanges();
                    throw;
                }
            }

            if (needSave)
            {
                try
                {
                    base.Log.LogMessage("Committing " + _artifactName + " actions...");
                    catalog.SaveChanges();
                    base.Log.LogMessage("Committed " + _artifactName + " actions.");
                }
                catch (Exception)
                {
                    catalog.DiscardChanges();
                    throw;
                }
            }
        }

        protected ModeType GetMode(string mode)
        {
            if (!Enum.IsDefined(typeof(ModeType), mode))
            {
                throw new ArgumentOutOfRangeException("value", "Mode must be Deploy or Undeploy.");
            }

            return (ModeType)Enum.Parse(typeof(ModeType), mode);
        }

        protected ActionType GetAction(string action)
        {
            if (!Enum.IsDefined(typeof(ActionType), action))
            {
                throw new ArgumentOutOfRangeException("value", "Mode must be Enlist, Unenlist, Start, Stop, Enable or Disable.");
            }

            return (ActionType)Enum.Parse(typeof(ActionType), action);
        }

        private void CopyMetadata(ITaskItem item, Dictionary<string, object> metadata)
        {
            string[] KnownMetadata = GetKnownMetadata();

            foreach (string metadataKey in KnownMetadata)
            {
                string value = item.GetMetadata(metadataKey);

                if (!string.IsNullOrWhiteSpace(value))
                {
                    metadata[metadataKey] = value;
                }
            }
        }

        protected abstract Dictionary<string, Dictionary<string, object>> GetArtifactsWithDefaultAction(Application application);

        protected abstract string[] GetKnownMetadata();

        protected abstract bool ControlIndividualArtifact(string name, Dictionary<string, object> metadata, BtsCatalogExplorer catalog, Application app);
    }
}
