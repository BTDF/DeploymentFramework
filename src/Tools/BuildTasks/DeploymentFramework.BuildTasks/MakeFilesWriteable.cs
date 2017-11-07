// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
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
    public class MakeFilesWriteable : Task
    {
        private ITaskItem[] _inputFiles;

        [Required]
        public ITaskItem[] InputFiles
        {
            get { return _inputFiles; }
            set { _inputFiles = value; }
        }

        public override bool Execute()
        {
            foreach (ITaskItem inputFile in _inputFiles)
            {
                this.Log.LogMessage(MessageImportance.Normal, "Clearing file attributes for '{0}'.", inputFile.ItemSpec);
                if (File.Exists(inputFile.ItemSpec))
                {
                    File.SetAttributes(inputFile.ItemSpec, FileAttributes.Normal);
                }
            }

            return true;
        }
    }
}
