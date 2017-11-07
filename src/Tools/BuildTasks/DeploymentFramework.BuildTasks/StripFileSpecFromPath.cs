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
    public class StripFileSpecFromPath : Task
    {
        private string _basePath;
        private string _path;

        [Required]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        [Output]
        public string BasePath
        {
            get { return _basePath; }
        }
	
        public override bool Execute()
        {
            int index = _path.LastIndexOf('\\');

            if (index <= 0)
            {
                return true;
            }

            _basePath = _path.Substring(0, _path.Length - (_path.Length - index));

            return true;
        }
    }
}
