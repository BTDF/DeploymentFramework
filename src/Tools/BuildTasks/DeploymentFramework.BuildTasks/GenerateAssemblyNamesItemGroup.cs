// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.BizTalk.ExplorerOM;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace DeploymentFramework.BuildTasks
{
    public class GenerateAssemblyNamesItemGroup : Task
    {
        private ITaskItem[] _assembliesItemGroup;
        private ITaskItem[] _assemblyNamesItemGroup;

        [Output]
        public ITaskItem[] AssemblyNamesItemGroup
        {
            get { return _assemblyNamesItemGroup; }
        }

        [Required]
        public ITaskItem[] SourceAssemblies
        {
            get { return _assembliesItemGroup; }
            set { _assembliesItemGroup = value; }
        }

        public override bool Execute()
        {
            List<TaskItem> assemblyNamesItemGroup = new List<TaskItem>();

            foreach (ITaskItem sourceAssembly in _assembliesItemGroup)
            {
                string assemblyName = System.Reflection.AssemblyName.GetAssemblyName(sourceAssembly.ItemSpec).FullName;

                TaskItem anti = new TaskItem(assemblyName);
                assemblyNamesItemGroup.Add(anti);

                this.Log.LogMessage(MessageImportance.Normal, "Adding assembly name '{0}' to item group.", anti.ItemSpec);
            }

            _assemblyNamesItemGroup = assemblyNamesItemGroup.ToArray();

            return true;
        }
    }
}
