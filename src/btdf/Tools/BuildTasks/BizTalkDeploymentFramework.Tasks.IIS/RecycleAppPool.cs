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
using System.Threading;

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

                    const int MaxAttempts = 3;
                    int attemptCount = 0;

                    base.Log.LogMessage("Recycling IIS application pool '" + appPoolName + "'...");

                    while (true)
                    {
                        try
                        {
                            attemptCount++;
                            appPool.Recycle();
                            break;
                        }
                        catch (Exception)
                        {
                            if (attemptCount == MaxAttempts)
                            {
                                base.Log.LogError("Failed to recycle IIS application pool '{0}'.", appPoolName);
                                return false;
                            }

                            base.Log.LogMessage("Recycle IIS application pool '" + appPoolName + "' failed. Trying again...");
                            Thread.Sleep(1000);
                        }
                    }
                    
                    base.Log.LogMessage("Recycled IIS application pool '" + appPoolName + "'.");
                }
            }

            return true;
        }
    }
}
