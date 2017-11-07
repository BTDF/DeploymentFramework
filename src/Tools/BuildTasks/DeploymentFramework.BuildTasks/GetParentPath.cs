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
    public class GetParentPath : Task
    {
        private string _parentPath;
        private string _path;

        [Required]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        [Output]
        public string ParentPath
        {
            get { return _parentPath; }
            set { _parentPath = value; }
        }
	
        public override bool Execute()
        {
            _parentPath = System.IO.Directory.GetParent(_path).FullName;
            return true;
        }
    }
}
