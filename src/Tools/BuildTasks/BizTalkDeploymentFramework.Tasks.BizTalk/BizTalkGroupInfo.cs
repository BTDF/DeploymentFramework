// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;

using Microsoft.Win32;

namespace DeploymentFramework.BuildTasks
{
	/// <summary>
	/// Summary description for BizTalkGroupInfo.
	/// </summary>
	public class BizTalkGroupInfo
	{
      public static string GroupDBServerName
      {
         get
         {
            using(RegistryKey rk = Registry.LocalMachine)
            {
               using(RegistryKey rk2 = rk.OpenSubKey(@"SOFTWARE\Microsoft\BizTalk Server\3.0\Administration"))
               {
                  string groupServerName = (string)rk2.GetValue("MgmtDBServer");
                  return groupServerName;
               }
            }
         }
      }

      public static string GroupMgmtDBName
      {
         get
         {
            using(RegistryKey rk = Registry.LocalMachine)
            {
               using(RegistryKey rk2 = rk.OpenSubKey(@"SOFTWARE\Microsoft\BizTalk Server\3.0\Administration"))
               {
                  string db = (string)rk2.GetValue("MgmtDBName");
                  return db;
               }
            }
         }
      }
	}
}
