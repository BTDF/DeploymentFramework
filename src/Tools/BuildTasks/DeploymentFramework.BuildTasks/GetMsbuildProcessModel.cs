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

namespace DeploymentFramework.BuildTasks
{
    public class GetMsbuildProcessModel : Task
    {
        private bool _is64BitProcess;

        [Output]
        public bool Is64BitProcess
        {
            get { return _is64BitProcess; }
        }

        public override bool Execute()
        {
            _is64BitProcess = (sizeof(int) > 4);

            return true;
        }
    }
}
