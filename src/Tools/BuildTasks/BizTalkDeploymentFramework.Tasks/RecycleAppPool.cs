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
    /// <summary>
    /// Recycle IIS application pool(s)
    /// </summary>
    public class RecycleAppPool : Task
    {
        [Required]
        public ITaskItem[] Items { get; set; }

        public override bool Execute()
        {
            using (ServerManager mgr = new ServerManager())
            {
                foreach (ITaskItem ti in this.Items)
                {
                    string appPoolName = ti.ItemSpec;

                    ApplicationPool appPool = mgr.ApplicationPools[appPoolName];

                    if (appPool == null)
                    {
                        continue;
                    }

                    base.Log.LogMessage("Recycling IIS application pool '" + appPoolName + "'...");
                    appPool.Recycle();
                    base.Log.LogMessage("Recycled IIS application pool '" + appPoolName + "'.");
                }
            }

            return true;
        }
    }
}
