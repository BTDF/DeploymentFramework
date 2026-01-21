using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSOSettingsFileManager;
using System.Collections;

namespace SSOSettingsFileManager.UnitTests
{
    [TestClass]
    public class SSOSettingsFileReaderUnitTests
    {
        private const string AppName = "SSOSettingsFileReaderUnitTests";
        private const string DefaultSettings =
            @"<settings><property name=""StringValue"">BizTalkSample</property><property name=""Int32Value"">100</property><property name=""ssoAppUserGroup"">BizTalk Application Users</property><property name=""ssoAppAdminGroup"">BizTalk Server Administrators</property></settings>";

        [TestMethod]
        public void ReadString()
        {
            try
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
            catch { }

            try
            {
                SSOSettingsManager.CreateApp(AppName, "BizTalk Application Users", "BizTalk Server Administrators");

                SSOSettingsManager.WriteRawSettings(AppName, DefaultSettings);

                string value = SSOSettingsFileReader.ReadString(AppName, "StringValue");

                Assert.IsTrue(string.Compare(@"BizTalkSample", value, true) == 0);
            }
            finally
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
        }

        [TestMethod]
        public void ReadInt32()
        {
            try
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
            catch { }

            try
            {
                SSOSettingsManager.CreateApp(AppName, "BizTalk Application Users", "BizTalk Server Administrators");

                SSOSettingsManager.WriteRawSettings(AppName, DefaultSettings);

                int value = SSOSettingsFileReader.ReadInt32(AppName, "Int32Value");

                Assert.AreEqual(100, value);
            }
            finally
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
        }

        [TestMethod]
        public void ReadAll()
        {
            try
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
            catch { }

            try
            {
                SSOSettingsManager.CreateApp(AppName, "BizTalk Application Users", "BizTalk Server Administrators");

                SSOSettingsManager.WriteRawSettings(AppName, DefaultSettings);

                Hashtable values = SSOSettingsFileReader.Read(AppName);

                Assert.IsTrue(values.Count == 4);
            }
            finally
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadInvalid()
        {
            try
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
            catch { }

            try
            {
                SSOSettingsManager.CreateApp(AppName, "BizTalk Application Users", "BizTalk Server Administrators");

                SSOSettingsManager.WriteRawSettings(AppName, DefaultSettings);

                SSOSettingsFileReader.ReadString(AppName, "NoValue");
            }
            finally
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
        }

        [TestMethod]
        public void ClearCache()
        {
            try
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
            catch { }

            try
            {
                SSOSettingsManager.CreateApp(AppName, "BizTalk Application Users", "BizTalk Server Administrators");

                SSOSettingsManager.WriteRawSettings(AppName, DefaultSettings);

                string value1 = SSOSettingsFileReader.ReadString(AppName, "StringValue");
                SSOSettingsManager.WriteSetting(AppName, "StringValue", "TestValue");
                string value2 = SSOSettingsFileReader.ReadString(AppName, "StringValue");

                Assert.IsTrue(string.Compare(value1, value2, true) == 0);

                SSOSettingsFileReader.ClearCache(AppName);
                string value3 = SSOSettingsFileReader.ReadString(AppName, "StringValue");

                Assert.IsTrue(string.Compare(value3, value2, true) != 0);
            }
            finally
            {
                SSOSettingsManager.DeleteApp(AppName);
            }
        }
    }
}
