// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace DeploymentFramework.BuildTasks
{
    public class GeneratePdbCopyItemGroups : Task
    {
        private ITaskItem[] _assembliesItemGroup;
        private ITaskItem[] _sourceItemGroup;
        private ITaskItem[] _destinationItemGroup;

        [Output]
        public ITaskItem[] SourceItemGroup
        {
            get { return _sourceItemGroup; }
        }

        [Output]
        public ITaskItem[] DestinationItemGroup
        {
            get { return _destinationItemGroup; }
        }

        [Required]
        public ITaskItem[] SourceAssemblies
        {
            get { return _assembliesItemGroup; }
            set { _assembliesItemGroup = value; }
        }

        public override bool Execute()
        {
            List<TaskItem> sourceItemGroup = new List<TaskItem>();
            List<TaskItem> destinationItemGroup = new List<TaskItem>();

            foreach (ITaskItem sourceAssembly in _assembliesItemGroup)
            {
                string destinationPath = Path.ChangeExtension(GetGacPathHelper.GetGacPath.GetPath(sourceAssembly.ItemSpec), ".pdb");

                TaskItem dti = new TaskItem(destinationPath);
                destinationItemGroup.Add(dti);

                string sourcePath = Path.ChangeExtension(sourceAssembly.ItemSpec, ".pdb");

                TaskItem sti = new TaskItem(sourcePath);
                sourceItemGroup.Add(sti);

                this.Log.LogMessage(MessageImportance.Normal, "Preparing to copy source file '{0}' to GAC path '{1}'.", sti.ItemSpec, dti.ItemSpec);
            }

            _sourceItemGroup = sourceItemGroup.ToArray();
            _destinationItemGroup = destinationItemGroup.ToArray();

            return true;
        }
    }
}
