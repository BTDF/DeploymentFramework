// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// Loads a BizTalk binding XML file, locates all FILE adapter paths, ensures that they exist and sets permissions on them.
    /// </summary>
    public class SetUpFileAdapterPaths : Task
    {
        private enum ModeType
        {
            SetUp,
            DeleteIfEmpty,
            DeleteRecursive
        }

        private ModeType _mode;
        private string _bindingFilePath;
        private string _userNameForFullControl;

        [Required]
        public string BindingFilePath
        {
            get { return _bindingFilePath; }
            set { _bindingFilePath = value; }
        }

        [Required]
        public string Mode
        {
            get { return _mode.ToString(); }
            set
            {
                if (!Enum.IsDefined(typeof(ModeType), value))
                {
                    throw new ArgumentOutOfRangeException("value", "Mode must be SetUp, DeleteIfEmpty or DeleteRecursive.");
                }

                _mode = (ModeType)Enum.Parse(typeof(ModeType), value);
            }
        }

        public string UserNameForFullControl
        {
            get { return _userNameForFullControl; }
            set { _userNameForFullControl = value; }
        }

        public override bool Execute()
        {
            this.Log.LogMessage(MessageImportance.Normal, "Configuring FILE adapter physical paths from binding file {0}...", _bindingFilePath);

            XmlDocument bindingDoc = new XmlDocument();
            bindingDoc.Load(_bindingFilePath);

            XmlNodeList nodesToUpdate = null;

            this.Log.LogMessage(MessageImportance.Normal, "  Processing FILE adapter physical paths (Send)...");
            nodesToUpdate = bindingDoc.SelectNodes("//SendPortCollection/SendPort/*/TransportType[@Name='FILE']");
            SetUpPaths(nodesToUpdate);

            this.Log.LogMessage(MessageImportance.Normal, "  Processing FILE adapter physical paths (Receive)...");
            nodesToUpdate = bindingDoc.SelectNodes("//ReceivePortCollection/ReceivePort/ReceiveLocations/ReceiveLocation/ReceiveLocationTransportType[@Name='FILE']");
            SetUpPaths(nodesToUpdate);

            return true;
        }

        private void SetUpPaths(XmlNodeList transportTypeNodes)
        {
            foreach (XmlElement transportTypeNode in transportTypeNodes)
            {
                // Get the Address node that contains the file path (parent of the transport type node, then down to Address)
                XmlElement addressElement = transportTypeNode.ParentNode.SelectSingleNode("Address") as XmlElement;

                if (addressElement == null || addressElement.IsEmpty)
                {
                    continue;
                }

                // The file path is the inner text of the Address node
                string originalAddress = addressElement.InnerText;

                if (string.IsNullOrEmpty(originalAddress))
                {
                    continue;
                }

                // Strip off the filespec from the end of the path (*.xml, %MessageID%.xml, etc.)
                string basePath = Path.GetDirectoryName(originalAddress);

                if (_mode == ModeType.SetUp)
                {
                    ConfigurePath(basePath);
                }
                else
                {
                    DeletePath(basePath);
                }
            }
        }

        private void ConfigurePath(string path)
        {
            this.Log.LogMessage(MessageImportance.Normal, "     Setting up path '{0}'...", path);

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    this.Log.LogMessage(MessageImportance.Normal, "       Created directory.");
                }
                catch (Exception ex)
                {
                    this.Log.LogWarning("Could not create path {0}.", path);
                    this.Log.LogWarningFromException(ex, false);
                }
            }
            else
            {
                this.Log.LogMessage(MessageImportance.Normal, "       Directory already exists.");
            }

            if (!string.IsNullOrEmpty(_userNameForFullControl))
            {
                if (IsUncOrNetworkPath(path))
                {
                    this.Log.LogMessage(MessageImportance.Normal, "       Cannot grant permissions because this is a UNC or network path.", _userNameForFullControl, path);
                }
                else
                {
                    try
                    {
                        SetDirectorySecurity(path, _userNameForFullControl, FileSystemRights.FullControl, AccessControlType.Allow);
                        this.Log.LogMessage(MessageImportance.Normal, "       Granted '{0}' Full Access permissions.", _userNameForFullControl);
                    }
                    catch (Exception ex)
                    {
                        this.Log.LogWarning("Could not grant Full Access permissions to {0} on path {1}.", _userNameForFullControl, path);
                        this.Log.LogWarningFromException(ex, false);
                        // Continue
                    }
                }
            }
        }

        private bool IsUncOrNetworkPath(string path)
        {
            if (path.StartsWith("\\\\"))
            {
                return true;
            }

            if (Path.IsPathRooted(path))
            {
                try
                {
                    DriveInfo di = new DriveInfo(Path.GetPathRoot(path).Substring(0, 1));

                    if (di.DriveType == DriveType.Network)
                    {
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            return false;
        }

        private void DeletePath(string path)
        {
            this.Log.LogMessage(MessageImportance.Normal, "     Deleting path {0}...", path);

            if (!Directory.Exists(path))
            {
                return;
            }

            try
            {
                if (_mode == ModeType.DeleteIfEmpty)
                {
                    Directory.Delete(path, false);
                }
                else if (_mode == ModeType.DeleteRecursive)
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                this.Log.LogWarning("Could not delete path {0}.", path);
                this.Log.LogWarningFromException(ex, false);
            }
        }

        public static void SetDirectorySecurity(string path, string userName, FileSystemRights rights, AccessControlType controlType)
        {
            // Sets an ACL entry on the specified directory for the specified account.

            // Get a DirectorySecurity object that represents the current security settings.
            DirectorySecurity dSecurity = Directory.GetAccessControl(path);

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.AddAccessRule(
                new FileSystemAccessRule(
                    userName, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, controlType));

            // Set the new access settings.
            Directory.SetAccessControl(path, dSecurity);
        }
    }
}
