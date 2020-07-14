// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.IO;
using System.Reflection;

namespace GetGacPathHelper
{
    public class GetGacPath
    {
        public static string GetPath(string assemblyPath)
        {
            Uri executingAssemblyCodeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase);

            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = Path.GetDirectoryName(executingAssemblyCodeBase.LocalPath);

            string path = null;
            AppDomain ad = null;

            try
            {
                ad = AppDomain.CreateDomain("GetGacPathTempAppDomain", null, ads);
                GetGacPathAppDomainHelper helper = (GetGacPathAppDomainHelper)
                    ad.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(GetGacPathAppDomainHelper).FullName);
                path = helper.GetGacPath(assemblyPath);
            }
            finally
            {
                if (ad != null)
                {
                    AppDomain.Unload(ad);
                }
            }

            return path;
        }
    }

    internal class GetGacPathAppDomainHelper : MarshalByRefObject
    {
        internal string GetGacPath(string assemblyPath)
        {
            string fullAssemblyPath = Path.GetFullPath(assemblyPath);
            Assembly a = Assembly.LoadFile(fullAssemblyPath);
            Uri cb = new Uri(a.CodeBase);
            return cb.LocalPath;
        }
    }
}
