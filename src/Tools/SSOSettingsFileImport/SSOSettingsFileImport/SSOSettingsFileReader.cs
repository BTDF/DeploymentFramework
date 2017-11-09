using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// This class is simply a forwarder (with a few extra convenience methods) that lives outside the
/// namespace so as to shorten up the syntax in the orchestration expression editor.
/// </summary>
public class SSOSettingsFileReader
{
    public static string ReadString(string affiliateApplication, string valueName)
    {
        return SSOSettingsFileManager.SSOSettingsFileReader.ReadString(affiliateApplication, valueName);
    }

    public static int ReadInt32(string affiliateApplication, string valueName)
    {
        return SSOSettingsFileManager.SSOSettingsFileReader.ReadInt32(affiliateApplication, valueName);
    }

    public static void ClearCache(string affiliateApplication)
    {
        SSOSettingsFileManager.SSOSettingsFileReader.ClearCache(affiliateApplication);
    }

    public static Hashtable Read(string affiliateApplication)
    {
        return SSOSettingsFileManager.SSOSettingsFileReader.Read(affiliateApplication);
    }

}

namespace SSOSettingsFileManager
{
    public class SSOSettingsFileReader
    {
        public static bool ExceptionOnNull = true;

        private const int RefreshIntervalSecs = 60;

        private static Hashtable _hashOfHashes = new Hashtable();
        private static DateTime _timeOfLastRefresh = DateTime.Now;

        public static string ReadString(string affiliateApplication, string valueName)
        {
            Hashtable ht = Read(affiliateApplication);
            string val = (string)ht[valueName];

            if (val == null && ExceptionOnNull)
            {
                throw new ArgumentException(
                    string.Format("Requested SSO value {0} is not available within affiliate app {1}",
                        valueName == null ? "empty" : valueName, affiliateApplication), "valueName");
            }

            return val;
        }

        public static int ReadInt32(string affiliateApplication, string valueName)
        {
            return Convert.ToInt32(ReadString(affiliateApplication, valueName));
        }

        public static Hashtable Read(string affiliateApplication)
        {
            return Read(affiliateApplication, false);
        }

        public static Hashtable Read(string affiliateApplication, bool enableRemoteAccess)
        {
            lock (_hashOfHashes)
            {
                // Clear all caches if the refresh interval has expired.
                ClearAllCachesIfRefreshIntervalExpired();

                // Check to see if the application is in the table.
                Hashtable htToReturn = (Hashtable)_hashOfHashes[affiliateApplication];
                if (htToReturn == null)
                {
                    htToReturn = SSOSettingsManager.GetSettings(affiliateApplication, enableRemoteAccess);
                    _hashOfHashes[affiliateApplication] = htToReturn;
                }

                return htToReturn;
            }
        }

        public static void ClearCache(string affiliateApplication)
        {
            lock (_hashOfHashes)
            {
                _hashOfHashes[affiliateApplication] = null;
            }
        }

        [Obsolete]
        public static string ReadValue(string affiliateApplication, string valueName)
        {
            return ReadString(affiliateApplication, valueName);
        }

        [Obsolete]
        public static void Update(string affiliateApplication, Hashtable ht)
        {
            SSOSettingsManager.WriteSettings(affiliateApplication, ht);
            ClearCache(affiliateApplication);
        }

        /// <summary>
        /// Will clear all the caches after the refresh interval has expired.
        /// It is assumed that a lock has been acquired prior to calling this function.
        /// </summary>
        private static void ClearAllCachesIfRefreshIntervalExpired()
        {
            // First get the elapsed time in seconds.
            TimeSpan elapsed = DateTime.Now - _timeOfLastRefresh;

            if (elapsed.TotalSeconds >= RefreshIntervalSecs)
            {
                _hashOfHashes.Clear();
                _timeOfLastRefresh = DateTime.Now;
            }
        }
    }
}
