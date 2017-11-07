// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SSOSettingsFileManager
{
    public static class SSOSettingsManager
    {
        internal static void CreateApp(string appName, string userGroup, string adminGroup)
        {
            SSOHelper.CreateApp(appName, userGroup, adminGroup);
        }

        internal static void DeleteApp(string appName)
        {
            SSOHelper.DeleteApp(appName);
        }

        internal static bool AppExists(string appName)
        {
            return SSOHelper.AppExists(appName);
        }

        internal static Hashtable GetSettings(string affiliateApplication, bool enableRemoteAccess)
        {
            string propertyValue = GetRawSettings(affiliateApplication, enableRemoteAccess);

            // Now we have what should be a settings file in hand as a string...
            // Need to deserialize it...
            settings appSettings = null;
            XmlSerializer serializer = new XmlSerializer(typeof(settings));
            StringReader stringReader = new StringReader(propertyValue);
            appSettings = (settings)serializer.Deserialize(stringReader);

            // Get a hashtable from the property collection and return it.
            Hashtable htToReturn = new Hashtable();
            foreach (property prop in appSettings.propertyCollection)
            {
                htToReturn[prop.name] = prop.Value;
            }

            return htToReturn;
        }

        internal static string GetRawSettings(string affiliateApplication, bool enableRemoteAccess)
        {
            return SSOHelper.GetConfigInfo(affiliateApplication, enableRemoteAccess);
        }

        internal static void WriteSettings(string affiliateApplication, Hashtable ht)
        {
            settings inSettings = new settings();

            foreach (object key in ht.Keys)
            {
                property prop = new property();
                prop.name = key.ToString();
                prop.Value = (string)ht[key];
                inSettings.propertyCollection.Add(prop);
            }

            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            XmlSerializer serializer = new XmlSerializer(typeof(settings));
            serializer.Serialize(writer, inSettings);

            SSOHelper.SaveConfigInfo(affiliateApplication, sb.ToString());
        }

        internal static void WriteRawSettings(string affiliateApplication, string settingsXml)
        {
            SSOHelper.SaveConfigInfo(affiliateApplication, settingsXml);
        }

        public static void WriteSetting(string affiliateApplication, string propertyName, string propertyValue)
        {
            Hashtable ht = GetSettings(affiliateApplication, false);
            ht[propertyName] = propertyValue;
            WriteSettings(affiliateApplication, ht);
        }
    }
}
