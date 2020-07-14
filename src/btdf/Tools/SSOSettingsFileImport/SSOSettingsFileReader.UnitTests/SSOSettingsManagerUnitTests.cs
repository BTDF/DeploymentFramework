using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSOSettingsFileManager;
using System.Collections;

namespace SSOSettingsFileManager.UnitTests
{
    [TestClass]
    public class SSOSettingsManagerUnitTests
    {
        private const string AppName = "SSOSettingsFileManagerUnitTests";
        private const string DefaultSettings = @"<settings><property name=""FileSendLocation"">c:\temp\BizTalkSample</property><property name=""ssoAppUserGroup"">BizTalk Application Users</property><property name=""ssoAppAdminGroup"">BizTalk Server Administrators</property></settings>";

        [TestMethod]
        public void WriteRawSettings()
        {
            try
            {
                SSOSettingsManager.CreateApp(AppName, "BizTalk Application Users", "BizTalk Server Administrators");

                SSOSettingsManager.WriteRawSettings(AppName, DefaultSettings);

                string settings = SSOSettingsManager.GetRawSettings(AppName, false);

                Assert.IsTrue(string.Compare(DefaultSettings, settings, true) == 0);
            }
            finally
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
        }

        [TestMethod]
        public void WriteSettings()
        {
            try
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
            catch {}

            try
            {
                SSOSettingsManager.CreateApp(AppName, "BizTalk Application Users", "BizTalk Server Administrators");

                SSOSettingsManager.WriteRawSettings(AppName, DefaultSettings);

                for (int x = 0; x < 50; x++)
                {
                    SSOSettingsManager.WriteSetting(AppName, "TestProperty" + x.ToString(), "TestValue" + x.ToString());
                    Console.WriteLine("Wrote setting " + "TestProperty" + x.ToString());
                }

                Hashtable settings = SSOSettingsManager.GetSettings(AppName, false);

                Assert.IsTrue(string.Compare("TestValue0", settings["TestProperty0"].ToString(), true) == 0);
            }
            finally
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
        }
    }
}
