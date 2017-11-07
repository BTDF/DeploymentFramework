// Scott Colestock / traceofthought.net

using System;

using Microsoft.Win32;

namespace BizTalk.NAnt.Tasks
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
