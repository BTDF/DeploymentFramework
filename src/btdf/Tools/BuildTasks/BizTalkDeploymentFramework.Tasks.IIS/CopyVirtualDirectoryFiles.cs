// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text.RegularExpressions;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Copies IIS application files to redist folder for MSI
    /// </summary>
    public class CopyVirtualDirectoryFiles : Task
    {
        [Required]
        public string MSBuildProjectDirectory { get; set; }

        [Required]
        public string RedistPath { get; set; }

        [Required]
        public ITaskItem[] Items { get; set; }

        public override bool Execute()
        {
            // Convert a relative path to an absolute path
            if (!Path.IsPathRooted(this.RedistPath))
            {
                string currentDirectory = Environment.CurrentDirectory;
                Environment.CurrentDirectory = this.MSBuildProjectDirectory;
                this.RedistPath = new DirectoryInfo(this.RedistPath).FullName;
                Environment.CurrentDirectory = currentDirectory;
            }

            foreach (ITaskItem ti in this.Items)
            {
                string physicalPath = ti.GetMetadata("PhysicalPath");
                string virtualPath = ti.GetMetadata("VirtualPath");
                string exclusionPatterns = ti.GetMetadata("Exclusions");

                if (string.IsNullOrWhiteSpace(physicalPath) || string.IsNullOrWhiteSpace(virtualPath))
                {
                    base.Log.LogError("Required metadata values PhysicalPath or VirtualPath are missing.");
                    return false;
                }

                // Convert a relative path to an absolute path
                if (!Path.IsPathRooted(physicalPath))
                {
                    string currentDirectory = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = this.MSBuildProjectDirectory;
                    physicalPath = new DirectoryInfo(physicalPath).FullName;
                    Environment.CurrentDirectory = currentDirectory;
                }

                List<Regex> exclusionRegExes = new List<Regex>();

                if (!string.IsNullOrWhiteSpace(exclusionPatterns))
                {
                    string[] exclusionPatternsSplit = exclusionPatterns.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string exclusionPattern in exclusionPatternsSplit)
                    {
                        string tempExclusionPattern = exclusionPattern.Trim();

                        if (tempExclusionPattern.StartsWith("\\"))
                        {
                            tempExclusionPattern = tempExclusionPattern.Remove(0, 1);
                        }
                        if (tempExclusionPattern.EndsWith("\\"))
                        {
                            tempExclusionPattern = tempExclusionPattern.Remove(tempExclusionPattern.Length - 1, 1);
                        }

                        tempExclusionPattern = Path.Combine(physicalPath, tempExclusionPattern);
                        tempExclusionPattern = tempExclusionPattern.Replace(@"\", @"\\");
                        tempExclusionPattern = tempExclusionPattern.Replace(@"*.", @"[^\\/:*?<>|]+"); // Match all valid Windows filename chars

                        exclusionRegExes.Add(new Regex(tempExclusionPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline));
                    }
                }

                virtualPath = virtualPath.Trim();

                if (!virtualPath.StartsWith("/"))
                {
                    virtualPath = "/" + virtualPath;
                }

                if (virtualPath.EndsWith("/"))
                {
                    virtualPath = virtualPath.TrimEnd('/');
                }

                int virtualPathSegmentCount = virtualPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length;

                // Extract the last virtualPathSegmentCount number of segments from the physical path
                // If virtualPath is /a/b/c and physicalPath is ..\k\x\y\z, take x\y\z because virtualPath has three segments
                int lastIndexOfStart = physicalPath.Length - 1;

                for (int index = 0; index < virtualPathSegmentCount; index++)
                {
                    lastIndexOfStart = physicalPath.LastIndexOf('\\', lastIndexOfStart) - 1;
                }

                string physicalPathSubDir = physicalPath.Remove(0, lastIndexOfStart + 1);

                if (physicalPathSubDir.StartsWith("\\"))
                {
                    physicalPathSubDir = physicalPathSubDir.Remove(0, 1);
                }

                string targetDir = Path.Combine(this.RedistPath, physicalPathSubDir);

                DirectoryCopy(physicalPath, targetDir, exclusionRegExes);
            }

            return true;
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, List<Regex> filters)
        {
            if (IsMatchForFilter(sourceDirName, filters))
            {
                return;
            }

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: " + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                base.Log.LogMessage("Creating directory \"" + destDirName + "\".");
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if ((file.Attributes & FileAttributes.Hidden) == 0 && (file.Attributes & FileAttributes.System) == 0)
                {
                    if (IsMatchForFilter(file.FullName, filters))
                    {
                        continue;
                    }

                    string temppath = Path.Combine(destDirName, file.Name);
                    base.Log.LogMessage("Copying file from \"" + file.FullName + "\" to \"" + temppath + "\".");
                    file.CopyTo(temppath, true);
                }
            }

            // Copy subdirectories and their contents to new location
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, filters);
            }
        }

        private bool IsMatchForFilter(string path, List<Regex> filters)
        {
            foreach (Regex r in filters)
            {
                if (r.IsMatch(path))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
