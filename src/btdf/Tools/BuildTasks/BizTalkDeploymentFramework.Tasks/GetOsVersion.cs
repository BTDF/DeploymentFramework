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

namespace DeploymentFramework.BuildTasks
{
    public class GetOsVersion : Task
    {
        private string _oSVersion;
        private bool _is64BitOperatingSystem;
        private string _iisMajorVersion;

        [Output]
        public bool Is64BitOperatingSystem
        {
            get { return _is64BitOperatingSystem; }
        }

        [Output]
        public string OsVersion
        {
            get { return _oSVersion; }
        }

        [Output]
        public string IisMajorVersion
        {
            get { return _iisMajorVersion; }
        }

        public override bool Execute()
        {
            OsVersionInfo ovi = new OsVersionInfo();

            _is64BitOperatingSystem = ovi.Is64BitOperatingSystem;
            _oSVersion = ovi.OsVersion;
            _iisMajorVersion = ovi.IisMajorVersion;

            return true;
        }
    }
}
