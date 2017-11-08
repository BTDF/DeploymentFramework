// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Management;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DeploymentFramework.BuildTasks
{
    public class ControlBizTalkHostInstance : Task
    {
        private enum ModeType
        {
            Start,
            Stop,
            Restart
        }

        private enum ServiceStateType : uint
        {
            Stopped = 1,
            StartPending = 2,
            StopPending = 3,
            Running = 4,
            ContinuePending = 5,
            PausePending = 6,
            Paused = 7,
            Unknown = 8
        }

        private enum ClusterInstanceType : uint
        {
            Unclustered = 0,
            Clustered = 1,
            ClusteredVirtual = 2
        }

        private ModeType _mode;
        private ITaskItem[] _bizTalkHostNamesItemGroup;

        [Required]
        public string Mode
        {
            get { return _mode.ToString(); }
            set
            {
                if (!Enum.IsDefined(typeof(ModeType), value))
                {
                    throw new ArgumentOutOfRangeException("value", "Mode must be Start, Stop or Restart.");
                }

                _mode = (ModeType)Enum.Parse(typeof(ModeType), value);
            }
        }

        public ITaskItem[] HostNames
        {
            get { return _bizTalkHostNamesItemGroup; }
            set { _bizTalkHostNamesItemGroup = value; }
        }

        public override bool Execute()
        {
            try
            {
                string query = null;

                if (_bizTalkHostNamesItemGroup == null)
                {
                    query = "Select * from MSBTS_HostInstance where HostType=1 and (ClusterInstanceType=0 or ClusterInstanceType=1 or ClusterInstanceType=2)";
                    ControlHostInstances(query);
                }
                else
                {
                    foreach (ITaskItem ti in _bizTalkHostNamesItemGroup)
                    {
                        query = "Select * from MSBTS_HostInstance where HostType=1  and (ClusterInstanceType=0 or ClusterInstanceType=1 or ClusterInstanceType=2) and HostName=\"" + ti.ItemSpec + "\"";
                        ControlHostInstances(query);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }
        }

        private void ControlHostInstances(string query)
        {
            //Create EnumerationOptions and run wql query
            EnumerationOptions enumOptions = new EnumerationOptions();
            enumOptions.ReturnImmediately = false;
            enumOptions.Rewindable = false;

            //Search for all HostInstances of 'InProcess' type in the Biztalk namespace scope
            using (ManagementObjectSearcher searchObject =
                new ManagementObjectSearcher("root\\MicrosoftBizTalkServer", query, enumOptions))
            {
                string activeNodeName = null;

                //Enumerate through the result set and start each HostInstance if it is already stopped
                foreach (ManagementObject inst in searchObject.Get())
                {
                    string runningServer = (string)inst["RunningServer"];
                    string hostName = (string)inst["HostName"];

                    if ((bool)inst["IsDisabled"])
                    {
                        this.Log.LogMessage("Skipping disabled host instance: " + hostName + " on " + runningServer);
                        continue;
                    }

                    ServiceStateType currentServiceState = (ServiceStateType)(uint)inst["ServiceState"];
                    ClusterInstanceType clusterInstanceType = (ClusterInstanceType)(uint)inst["ClusterInstanceType"];

                    // Handle clustered host instances
                    if (clusterInstanceType == ClusterInstanceType.Clustered || clusterInstanceType == ClusterInstanceType.ClusteredVirtual)
                    {
                        this.Log.LogMessage("Host instance " + hostName + " has cluster type " + clusterInstanceType.ToString() + ".");

                        if (string.IsNullOrEmpty(activeNodeName))
                        {
                            activeNodeName = GetActiveClusterNodeName();
                        }

                        if (string.Compare(activeNodeName, runningServer, true) != 0)
                        {
                            this.Log.LogMessage("Skipping passive clustered host instance: " + hostName + " on " + runningServer);
                            continue;
                        }
                    }

                    //Check if ServiceState is not 'Stop Pending' and 'Stopped'
                    if ((_mode == ModeType.Restart || _mode == ModeType.Stop)
                        && (currentServiceState != ServiceStateType.Stopped && currentServiceState != ServiceStateType.StopPending))
                    {
                        if (_mode == ModeType.Stop)
                        {
                            this.Log.LogMessage("Stopping host instance: " + hostName + " on " + runningServer);
                        }
                        else if (_mode == ModeType.Restart)
                        {
                            this.Log.LogMessage("Restarting host instance: " + hostName + " on " + runningServer);
                        }

                        try
                        {
                            inst.InvokeMethod("Stop", null);
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.ToLowerInvariant().Contains("overlapped i/o operation is in progress".ToLowerInvariant()))
                            {
                                throw;
                            }
                        }

                        if (_mode == ModeType.Stop)
                        {
                            this.Log.LogMessage("Stopped host instance : " + hostName + " on " + runningServer);
                        }

                        inst.Get();
                        currentServiceState = (ServiceStateType)(uint)inst["ServiceState"];
                        runningServer = (string)inst["RunningServer"];
                    }

                    //Check if ServiceState is not 'Start Pending' and 'Running'
                    if ((_mode == ModeType.Restart || _mode == ModeType.Start)
                        && (currentServiceState != ServiceStateType.Running
                            && currentServiceState != ServiceStateType.StartPending
                            && currentServiceState != ServiceStateType.ContinuePending))
                    {
                        if (_mode == ModeType.Start)
                        {
                            this.Log.LogMessage("Starting host instance: " + hostName + " on " + runningServer);
                        }

                        try
                        {
                            inst.InvokeMethod("Start", null);
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.ToLowerInvariant().Contains("the cluster which this is part of was brought online".ToLowerInvariant()))
                            {
                                throw;
                            }
                            else
                            {
                                this.Log.LogMessage(ex.Message);
                            }
                        }

                        if (_mode == ModeType.Start)
                        {
                            this.Log.LogMessage("Started host instance : " + hostName + " on " + runningServer);
                        }
                        else if (_mode == ModeType.Restart)
                        {
                            this.Log.LogMessage("Restarted host instance : " + hostName + " on " + runningServer);
                        }
                    }
                }
            }
        }

        private string GetActiveClusterNodeName()
        {
            EnumerationOptions enumOptions = new EnumerationOptions();
            enumOptions.ReturnImmediately = false;
            enumOptions.Rewindable = false;

            this.Log.LogMessage("Querying localhost to request active cluster node...");

            //Search for all MSCluster_NodeToActiveGroup instances in the MSCluster namespace scope
            using (ManagementObjectSearcher searchObject =
                new ManagementObjectSearcher("\\\\localhost\\root\\MSCluster", "SELECT * FROM MSCluster_NodeToActiveGroup", enumOptions))
            {
                //Enumerate through the result set
                foreach (ManagementObject inst in searchObject.Get())
                {
                    // Data looks like MSCluster_Node.Name="node2" and indicates the active node
                    string groupComponent = (string)inst["GroupComponent"];

                    string[] groupSegments = groupComponent.Split('"', '\'');

                    this.Log.LogMessage("Active cluster node reported by localhost is " + groupSegments[1] + ".");
                    return groupSegments[1];
                }
            }

            this.Log.LogMessage("Failed to obtain active cluster node from localhost.");
            return "UNKNOWN";
        }
    }
}
