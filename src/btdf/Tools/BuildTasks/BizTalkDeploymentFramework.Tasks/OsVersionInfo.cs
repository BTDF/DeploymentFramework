// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace DeploymentFramework.BuildTasks
{
    internal class OsVersionInfo
    {
        internal enum Platform
        {
            X86,
            X64,
            Unknown
        }

        private string _oSVersion;
        private bool _is64BitOperatingSystem;
        private string _iisMajorVersion;

        public bool Is64BitOperatingSystem
        {
            get { return _is64BitOperatingSystem; }
        }

        public string OsVersion
        {
            get { return _oSVersion; }
        }

        public string IisMajorVersion
        {
            get { return _iisMajorVersion; }
        }

        internal OsVersionInfo()
        {
            _oSVersion = string.Format("{0}{1}", Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor);

            Platform pf = GetPlatform();
            _is64BitOperatingSystem = (pf == Platform.X64);

            if (Environment.OSVersion.Version.Major > 5)
            {
                _iisMajorVersion = "7";
            }
            else if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 2)
            {
                _iisMajorVersion = "6";
            }
            else
            {
                _iisMajorVersion = "5";
            }
        }

        private static Platform GetPlatform()
        {
            SafeNativeMethods.SYSTEM_INFO sysInfo = new SafeNativeMethods.SYSTEM_INFO();

            if (System.Environment.OSVersion.Version.Major > 5 ||
                (System.Environment.OSVersion.Version.Major == 5 && System.Environment.OSVersion.Version.Minor >= 1))
            {
                SafeNativeMethods.GetNativeSystemInfo(ref sysInfo);
            }
            else
            {
                SafeNativeMethods.GetSystemInfo(ref sysInfo);
            }

            switch (sysInfo.wProcessorArchitecture)
            {
                case SafeNativeMethods.PROCESSOR_ARCHITECTURE_IA64:
                case SafeNativeMethods.PROCESSOR_ARCHITECTURE_AMD64:
                    return Platform.X64;

                case SafeNativeMethods.PROCESSOR_ARCHITECTURE_INTEL:
                    return Platform.X86;

                default:
                    return Platform.Unknown;
            }
        }
    }
}
