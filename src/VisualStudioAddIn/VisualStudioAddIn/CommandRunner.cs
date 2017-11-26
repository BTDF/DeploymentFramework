// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;

namespace DeploymentFramework.VisualStudioAddIn
{
    internal class CommandRunner
    {
        internal int IsBusy = 0;

        private DTE2 _applicationObject;

        private delegate void RunHandler(string exePath, string arguments);

        internal CommandRunner(DTE2 applicationObject)
        {
            this._applicationObject = applicationObject;
        }

        internal void ExecuteBuild(string exePath, string arguments)
        {
            SetBusy();

            OutputWindow ow = _applicationObject.ToolWindows.OutputWindow;
            OutputWindowPane owP = GetOutputWindowPane("Deployment Framework for BizTalk");

            owP.Clear();
            owP.Activate();
            ow.Parent.Activate();

            RunHandler rh = new RunHandler(Run);
            AsyncCallback callback = new AsyncCallback(RunCallback);
            rh.BeginInvoke(exePath, arguments, callback, rh);
        }

        private void SetBusy()
        {
            Interlocked.CompareExchange(ref IsBusy, 1, 0);
        }

        private void SetFree()
        {
            Interlocked.CompareExchange(ref IsBusy, 0, 1);
        }

        private void RunCallback(IAsyncResult result)
        {
            RunHandler rh = result.AsyncState as RunHandler;

            try
            {
                rh.EndInvoke(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Deployment Framework for BizTalk: Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetFree();
            }
        }

        private void proc_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            WriteToOutputWindow(e.Data);
        }

        private void WriteToOutputWindow(string message)
        {
            OutputWindowPane owP = GetOutputWindowPane("Deployment Framework for BizTalk");
            owP.OutputString(message);
            owP.OutputString("\r\n");
        }

        private void Run(string exePath, string arguments)
        {
            RunProcess(exePath, arguments);
        }

        private void RunProcess(string exePath, string arguments)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();

            WriteToOutputWindow("Starting build...");
            WriteToOutputWindow(exePath + " " + arguments);
            WriteToOutputWindow(string.Empty);

            proc.StartInfo.FileName = exePath;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;

            proc.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(proc_OutputDataReceived);
            proc.Start();

            proc.BeginOutputReadLine();
            proc.WaitForExit();
        }

        private OutputWindowPane GetOutputWindowPane(string name)
        {
            OutputWindow ow = _applicationObject.ToolWindows.OutputWindow;

            foreach (OutputWindowPane owp in ow.OutputWindowPanes)
            {
                if (string.Compare(owp.Name, name, true) == 0)
                {
                    return owp;
                }
            }

            return ow.OutputWindowPanes.Add(name);
        }
    }
}
