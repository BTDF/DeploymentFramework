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
