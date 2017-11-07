//-----------------------------------------------------------------------
// <copyright file="Exists.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
// </copyright>
// <summary>Recycle an Application Pool</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Sdc.Tasks.Web.AppPool
{
    using System;
    using Microsoft.Build.Framework;
    using Web = Microsoft.Sdc.Tasks.Configuration.Web;

    /// <summary>
    /// Recycle an Application Pool
    /// </summary>
    /// <remarks>
    /// <code><![CDATA[<Web.AppPool.Recycle AppPoolName="appPoolName"/>]]></code>
    /// <para>where:</para>
    /// <para><i>appPoolName (Required)</i></para>
    /// <para>A valid, existing IIS pool name</para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// <Project>
    ///     <Target Name="Test" >
    ///         <Web.AppPool.Recycle AppPoolName="TestApplicationPool"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>    
    public class Recycle : TaskBase
    {
        private string name;
        private string machineName = "localhost";

        /// <summary>
        /// Machine to operate on
        /// </summary>
        public string MachineName
        {
            get { return this.machineName; }
            set { this.machineName = value; }
        }

        /// <summary>
        /// Name of the application pool to check for.
        /// </summary>
        /// <value>Any valid IIS application pool name</value>
        [Required]
        public string AppPoolName
        {
            get { return this.name ?? String.Empty; }

            set { this.name = value; }
        }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        protected override void InternalExecute()
        {
            Log.LogMessage(MessageImportance.Normal, string.Format("Attempting to recycle IIS AppPool {0} on {1}...", this.AppPoolName, this.MachineName));
            if (Web.AppPool.Exists(this.name, this.MachineName))
            {
                Web.AppPool appPool = Web.AppPool.Load(this.name, this.MachineName);
                appPool.Recycle();
                Log.LogMessage(MessageImportance.Normal, string.Format("Recycled IIS AppPool {0} on {1}.", this.AppPoolName, this.MachineName));
            }
            else
            {
                Log.LogWarning(string.Format("Could not recycle IIS AppPool {0} on {1} because it does not exist.", this.AppPoolName, this.MachineName));
            }
        }
    }
}