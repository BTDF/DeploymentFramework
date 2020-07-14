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
using Microsoft.Web.Administration;

namespace DeploymentFramework.BuildTasks
{
    public abstract class ConfigureIISTask : Task
    {
        protected enum ModeType
        {
            Unknown,
            Deploy,
            Undeploy
        }

        protected enum ActionType
        {
            None,
            Create,
            Delete,
            Update,
            CreateOrUpdate
        }

        private ModeType _mode;

        [Required]
        public string MSBuildProjectDirectory { get; set; }

        [Required]
        public ITaskItem[] Items { get; set; }

        [Required]
        public string Mode
        {
            get
            {
                return _mode.ToString();
            }
            set
            {
                if (!Enum.TryParse<ModeType>(value, true, out _mode))
                {
                    base.Log.LogError("Invalid Mode parameter value -- must be Deploy or Undeploy");
                }
            }
        }

        public override bool Execute()
        {
            bool result = true;

            using (ServerManager mgr = new ServerManager())
            {
                foreach (ITaskItem ti in this.Items)
                {
                    string actionStr;
                    ActionType action;

                    if (_mode == ModeType.Deploy)
                    {
                        actionStr = ti.GetMetadata("DeployAction");

                        if (!Enum.TryParse<ActionType>(actionStr, true, out action))
                        {
                            base.Log.LogError("Invalid DeployAction metadata value " + actionStr);
                            return false;
                        }

                        if (action == ActionType.None)
                        {
                            base.Log.LogMessage("Skipping '" + ti.ItemSpec + "' because DeployAction is set to None.");
                            continue;
                        }

                        result = Configure(mgr, ti, _mode, action);
                    }
                    else
                    {
                        actionStr = ti.GetMetadata("UndeployAction");

                        if (!Enum.TryParse<ActionType>(actionStr, true, out action))
                        {
                            base.Log.LogError("Invalid UndeployAction metadata value " + actionStr);
                            return false;
                        }

                        if (action == ActionType.None)
                        {
                            base.Log.LogMessage("Skipping '" + ti.ItemSpec + "' because UndeployAction is set to None.");
                            continue;
                        }

                        result = Configure(mgr, ti, _mode, action);
                    }

                    if (!result)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected abstract bool Configure(ServerManager mgr, ITaskItem ti, ModeType mode, ActionType action);
    }
}
