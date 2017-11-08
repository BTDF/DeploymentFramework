// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DeploymentFramework.VisualStudioAddIn
{
    internal class Util
    {
        internal static string GetMsBuildPath(VSVersion ideVersion)
        {
            const string VS2005KEY = @"SOFTWARE\Microsoft\VisualStudio\8.0\MSBuild";
            const string VS2008KEY = @"SOFTWARE\Microsoft\MSBuild\ToolsVersions\3.5";
            const string VS2010KEY = @"SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0";
            //const string VS2013KEY = @"SOFTWARE\Microsoft\MSBuild\ToolsVersions\12.0";

            if (ideVersion == VSVersion.Vs2008)
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(VS2008KEY, false))
                {
                    return string.Format("{0}MSBuild.exe", (string)rk.GetValue("MSBuildToolsPath"));
                }
            }
            else if (ideVersion == VSVersion.Vs2010 || ideVersion == VSVersion.Vs2012 || ideVersion == VSVersion.Vs2013)
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(VS2010KEY, false))
                {
                    return string.Format("{0}MSBuild.exe", (string)rk.GetValue("MSBuildToolsPath"));
                }
            }
            //else if (ideVersion == VSVersion.Vs2013)
            //{
            //    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(VS2013KEY, false))
            //    {
            //        return string.Format("{0}MSBuild.exe", (string)rk.GetValue("MSBuildToolsPath"));
            //    }
            //}
            else
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(VS2005KEY, false))
                {
                    return string.Format("{0}MSBuild.exe", (string)rk.GetValue("MSBuildBinPath"));
                }
            }
        }

        internal static string GetGacUtilPath()
        {
            string btdfInstallPath = GetDeploymentFrameworkInstallPath();

            if (string.IsNullOrEmpty(btdfInstallPath))
            {
                return null;
            }

            return string.Format("{0}Framework\\DeployTools\\GacUtil.exe", btdfInstallPath);
        }

        internal static string GetDeploymentFrameworkInstallPath()
        {
            string btdfInstallDir =
                (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\DeploymentFrameworkForBizTalk", "InstallPath", null);

            if (string.IsNullOrEmpty(btdfInstallDir))
            {
                MessageBox.Show(
                    "Cannot find Deployment Framework registry key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return btdfInstallDir;
        }

        internal static string GetDeploymentProjectPath(string solutionPath)
        {
            //string solutionPath = _applicationObject.Solution.FileName;
            string solutionFilenameNoExt = Path.GetFileNameWithoutExtension(solutionPath);

            // First look for <solutionNameNoExtension>.Deployment\<solutionNameNoExtension>.Deployment.btdfproj
            string projectPath = Path.Combine(Path.GetDirectoryName(solutionPath), solutionFilenameNoExt + ".Deployment");
            projectPath = Path.Combine(projectPath, solutionFilenameNoExt + ".Deployment.btdfproj");

            if (!File.Exists(projectPath))
            {
                // Next look for <solutionNameNoExtension>.Deployment\Deployment.btdfproj
                projectPath = Path.Combine(Path.GetDirectoryName(solutionPath), solutionFilenameNoExt + ".Deployment");
                projectPath = Path.Combine(projectPath, "Deployment.btdfproj");

                if (!File.Exists(projectPath))
                {
                    // Next look for Deployment\<solutionNameNoExtension>.Deployment.btdfproj
                    projectPath = Path.Combine(Path.GetDirectoryName(solutionPath), "Deployment");
                    projectPath = Path.Combine(projectPath, solutionFilenameNoExt + ".Deployment.btdfproj");

                    if (!File.Exists(projectPath))
                    {
                        // Next look for Deployment\Deployment.btdfproj
                        projectPath = Path.Combine(Path.GetDirectoryName(solutionPath), "Deployment");
                        projectPath = Path.Combine(projectPath, "Deployment.btdfproj");

                        if (!File.Exists(projectPath))
                        {
                            MessageBox.Show(
                                "Could not find a .btdfproj file for this solution. Valid locations relative to the solution root are: <solutionNameNoExtension>.Deployment\\<solutionNameNoExtension>.Deployment.btdfproj, <solutionNameNoExtension>.Deployment\\Deployment.btdfproj, Deployment\\<solutionNameNoExtension>.Deployment.btdfproj or Deployment\\Deployment.btdfproj.",
                                "Deployment Framework for BizTalk",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }

            return projectPath;
        }
    }
}
