// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.BizTalk.SSOClient.Interop;

namespace SSOSettingsFileManager
{
    /// <summary>
    /// Helper component for SSO interactions
    /// </summary>
    internal static class SSOHelper
    {
        /// <summary>
        /// IPropertyBag storage for SSO info
        /// </summary>
        private class SSOPropertyBag : IPropertyBag
        {
            Hashtable _props = new Hashtable();

            #region IPropertyBag Members

            /// <summary>
            /// Writes the specified name of the prop.
            /// </summary>
            /// <param name="propName">Name of the prop.</param>
            /// <param name="ptrVar">PTR var.</param>
            public void Write(string propName, ref object ptrVar)
            {
                _props[propName] = ptrVar;
            }

            /// <summary>
            /// Reads the specified name of the prop.
            /// </summary>
            /// <param name="propName">Name of the prop.</param>
            /// <param name="ptrVar">PTR var.</param>
            /// <param name="errorLog">Error log.</param>
            public void Read(string propName, out object ptrVar, int errorLog)
            {
                ptrVar = _props[propName];
            }

            #endregion
        }

        private const string PropName = "AppConfig";
        private const string InfoIdentifier = "{56D74464-67EA-464d-A9D4-3EBBA4090010}";

        internal static string GetConfigInfo(string affiliateApplication, bool enableRemoteAccess)
        {
            int ssoFlag = (enableRemoteAccess ? SSOFlag.SSO_FLAG_NONE : SSOFlag.SSO_FLAG_RUNTIME);

            object propertyValue;
            SSOPropertyBag bag = new SSOPropertyBag();

            ISSOConfigStore ssoConfigStore = new ISSOConfigStore();
            ssoConfigStore.GetConfigInfo(affiliateApplication, InfoIdentifier, ssoFlag, bag);

            bag.Read(PropName, out propertyValue, 0);

            return (string)propertyValue;
        }

        /// <summary>
        /// Saves the config info.
        /// </summary>
        internal static void SaveConfigInfo(string affiliateApplication, string settings)
        {
            object settingsObj = (object)settings;

            SSOPropertyBag bag = new SSOPropertyBag();
            bag.Write(PropName, ref settingsObj);

            int retryCounter = 5;

            ISSOConfigStore ssoConfigStore = new ISSOConfigStore();

            while (retryCounter >= 0)
            {
                try
                {
                    ssoConfigStore.SetConfigInfo(affiliateApplication, InfoIdentifier, bag);
                    break;
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    // Error Code = 'The external credentials in the SSO database are more recent.'
                    // This error occurs randomly and in virtually all cases, an immediate retry succeeds.
                    // Tried just about everything to prevent it with no luck, so retry seems to be the best option.
                    if (ex.ErrorCode != -1073731008)
                    {
                        throw;  // All other errors just rethrow
                    }

                    Trace.Write("Caught 'The external credentials in the SSO database are more recent' exception. ");

                    if (retryCounter == 0)
                    {
                        Trace.WriteLine("Exhausted retries.");
                        throw;
                    }
                    else
                    {
                        Trace.WriteLine("Retries remaining: " + retryCounter.ToString());
                    }
                }

                retryCounter--;
            }
        }

        internal static void DeleteApp(string appName)
        {
            if (AppExists(appName))
            {
                DeleteConfigInfo(appName);

                ISSOAdmin ssoAdmin = new ISSOAdmin();
                ssoAdmin.DeleteApplication(appName);
            }
        }

        // Borrowed largely from Jon Flanders' SSOAppConfigHelper object.
        internal static void CreateApp(string appName, string userGroup, string adminGroup)
        {
            const int Fields = 2;
            int flags = SSOFlag.SSO_FLAG_APP_CONFIG_STORE | SSOFlag.SSO_FLAG_APP_ALLOW_LOCAL | SSOFlag.SSO_FLAG_SSO_WINDOWS_TO_EXTERNAL;

            ISSOAdmin _ssoAdmin = new ISSOAdmin();
            _ssoAdmin.CreateApplication(
                appName, appName + " Configuration Data", "someone@microsoft.com", userGroup, adminGroup, flags, Fields);
            _ssoAdmin.CreateFieldInfo(appName, "someone@microsoft.com", SSOFlag.SSO_FLAG_NONE);
            _ssoAdmin.CreateFieldInfo(appName, PropName, SSOFlag.SSO_FLAG_NONE);

            _ssoAdmin.UpdateApplication(appName, null, null, null, null, SSOFlag.SSO_FLAG_ENABLED, SSOFlag.SSO_FLAG_ENABLED);
        }

        internal static bool AppExists(string appName)
        {
            string description, contact, grpName, grpAdmin;
            int flags, fields;

            ISSOAdmin ssoAdmin = new ISSOAdmin();
            try
            {
                ssoAdmin.GetApplicationInfo(appName, out description, out contact, out grpName, out grpAdmin, out flags, out fields);
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Deletes the config info.
        /// </summary>
        private static void DeleteConfigInfo(string affiliateApplication)
        {
            ISSOConfigStore ssoConfigStore = new ISSOConfigStore();
            ssoConfigStore.DeleteConfigInfo(affiliateApplication, InfoIdentifier);
        }
    }
}
