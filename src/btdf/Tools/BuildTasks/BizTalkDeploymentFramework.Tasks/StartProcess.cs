// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Diagnostics;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Start a process without redirecting the output
    /// </summary>
    public class StartProcess : Task
    {
        private string _command;
        private string _arguments;
        private string _async = "false";

        [Required]
        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        [Required]
        public string Arguments
        {
            get { return _arguments; }
            set { _arguments = value; }
        }

        public string Async
        {
            get { return _async; }
            set { _async = value; }
        }

        public override bool Execute()
        {
            bool runAsync = false;

            if (!string.IsNullOrEmpty(_async))
            {
                runAsync = bool.Parse(_async);
            }

            ProcessStartInfo psi = new ProcessStartInfo(_command, _arguments);
            psi.UseShellExecute = false;
            psi.RedirectStandardError = false;
            psi.RedirectStandardInput = false;
            psi.RedirectStandardOutput = false;

            Process p = Process.Start(psi);

            if (!runAsync)
            {
                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    //base.Log.LogCommandLine(MessageImportance.Low, _command + " " + _arguments);
                    //base.Log.LogError("The external program returned error code " + p.ExitCode + ".");
                    return false;
                }
            }

            return true;
        }
    }
}
