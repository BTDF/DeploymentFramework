// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Practices.ESB.Resolver;
using Microsoft.XLANGs.BaseTypes;

namespace DeploymentFrameworkForBizTalk.ESB.Resolver.Sso
{
    /// <summary>
    /// An ESB Toolkit 2.0 custom resolver that pulls data values from a BizTalk SSO affiliate application.
    /// The SSO affiliate application must be loaded with an XML file that contains name-value pairs
    /// that correspond to the SettingsFileGenerator.xml Excel spreadsheet. The Deployment Framework for BizTalk
    /// automatically loads the XML into the SSO affiliate application during deployment.
    /// </summary>
    public class ResolveProvider : IResolveProvider
    {
        #region IResolveProvider Members

        // <summary>
        // This is the main interface that is called from the ResolverMgr class.
        // This interface supports being called from an orchestration.
        // </summary>
        // <param name="resolverInfo">Configuration string containing configuration and resolver</param>.
        // <param name="message">XLANGMessage passed from orchestration</param>.
        // <returns>Dictionary object fully populated</returns>.
        public Dictionary<string, string> Resolve(ResolverInfo resolverInfo, XLANGMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Dictionary<string, string> resolverDictionary = null;

            try
            {
                Resolution resolution = new Resolution();

                ResolverMgr.SetContext(resolution, message);

                resolverDictionary = ResolveHelper(resolverInfo.Config, resolverInfo.Resolver, resolution);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("BizTalk Server", exception.ToString(), EventLogEntryType.Error);
                throw;
            }

            return resolverDictionary;
        }

        // <summary>
        // This is the main interface that is called from the ResolverMgr class.
        // This interface supports being called from a Web service interface.
        // </summary>
        // <param name="config">Configuration string entered into Web service as argument</param>.
        // <param name="resolver">Moniker representing the resolver to load</param>.
        // <param name="message">XML document passed from Web service</param>.
        // <returns>Dictionary object fully populated</returns>.
        public Dictionary<string, string> Resolve(string config, string resolver, XmlDocument message)
        {
            if (string.IsNullOrEmpty(config))
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(resolver))
            {
                throw new ArgumentNullException("resolver");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Dictionary<string, string> resolverDictionary = null;

            try
            {
                Resolution resolution = new Resolution();

                resolverDictionary = ResolveHelper(config, resolver, resolution);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("BizTalk Server", exception.ToString(), EventLogEntryType.Error);
                throw;
            }

            return resolverDictionary;
        }

        // <summary>
        // This is the main interface that is called from the ResolverMgr class.
        // This interface supports being called from a Microsoft BizTalk Server pipeline component.
        // </summary>
        // <param name="config">Configuration string entered into pipeline component as argument</param>
        // <param name="resolver">Moniker representing the resolver to load</param>.
        // <param name="message">IBaseMessage passed from pipeline</param>.
        // <param name="pipelineContext">IPipelineContext passed from pipeline</param>.
        // <returns>Dictionary object fully populated</returns>.
        public Dictionary<string, string> Resolve(string config, string resolver, IBaseMessage message, IPipelineContext pipelineContext)
        {
            if (string.IsNullOrEmpty(config))
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(resolver))
            {
                throw new ArgumentNullException("resolver");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (pipelineContext == null)
            {
                throw new ArgumentNullException("pipelineContext");
            }

            Dictionary<string, string> resolverDictionary = null;

            try
            {
                Resolution resolution = new Resolution();

                ResolverMgr.SetContext(resolution, message, pipelineContext);

                resolverDictionary = ResolveHelper(config, resolver, resolution);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("BizTalk Server", exception.ToString(), EventLogEntryType.Error);
                throw;
            }

            return resolverDictionary;
        }

        #endregion

        /// <summary>
        /// Internal shared implementation of the Resolve method. Retrieves the values configured by
        /// the developer (usually in the itinerary designer) for the BTDF-SSO resolver. Each value
        /// is used as a key to search the name-value dictionary stored in SSO (originating from the
        /// settings spreadsheet). The values matching each key are assigned into the ResolverMgr's
        /// endpoint properties collection.
        /// </summary>
        /// <param name="config">Resolver configuration string</param>
        /// <param name="resolver">Resolver name</param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        private Dictionary<string, string> ResolveHelper(string config, string resolver, Resolution resolution)
        {
            if (!resolver.Contains(ResolutionHelper.MonikerSeparator))
            {
                resolver = resolver + ResolutionHelper.MonikerSeparator;
            }

            Dictionary<string, string> resolverDictionary = new Dictionary<string, string>();

            try
            {
                Dictionary<string, string> facts = ResolutionHelper.GetFacts(config, resolver);

                Sso expr = new Sso();

                expr.AffiliateAppName = ResolverMgr.GetConfigValue(facts, false, "AffiliateAppName");

                expr.Action = ResolverMgr.GetConfigValue(facts, false, "Action");
                expr.EndpointConfig = ResolverMgr.GetConfigValue(facts, false, "EndpointConfig");
                expr.JaxRpcResponse = ResolverMgr.GetConfigValue(facts, false, "JaxRpcResponse");
                expr.MessageExchangePattern = ResolverMgr.GetConfigValue(facts, false, "MessageExchangePattern");
                expr.TargetNamespace = ResolverMgr.GetConfigValue(facts, false, "TargetNamespace");
                expr.TransformType = ResolverMgr.GetConfigValue(facts, false, "TransformType");
                expr.TransportLocation = ResolverMgr.GetConfigValue(facts, false, "TransportLocation");
                expr.TransportType = ResolverMgr.GetConfigValue(facts, false, "TransportType");

                string rowValue = null;

                if (!string.IsNullOrEmpty(expr.Action))
                {
                    rowValue = ReadValueFromSso(expr.AffiliateAppName, expr.Action);
                    ResolverMgr.SetEndpointProperties("Action", rowValue, resolution);
                }

                if (!string.IsNullOrEmpty(expr.EndpointConfig))
                {
                    rowValue = ReadValueFromSso(expr.AffiliateAppName, expr.EndpointConfig);
                    ResolverMgr.SetEndpointProperties("EndpointConfig", rowValue, resolution);
                }

                if (!string.IsNullOrEmpty(expr.JaxRpcResponse))
                {
                    // This one is just True/False, enforced by the itinerary designer.
                    //rowValue = ReadValueFromSso(expr.AffiliateAppName, expr.JaxRpcResponse);
                    ResolverMgr.SetEndpointProperties("JaxRpcResponse", expr.JaxRpcResponse, resolution);
                }

                if (!string.IsNullOrEmpty(expr.MessageExchangePattern))
                {
                    rowValue = ReadValueFromSso(expr.AffiliateAppName, expr.MessageExchangePattern);
                    ResolverMgr.SetEndpointProperties("MessageExchangePattern", rowValue, resolution);
                }

                if (!string.IsNullOrEmpty(expr.TargetNamespace))
                {
                    rowValue = ReadValueFromSso(expr.AffiliateAppName, expr.TargetNamespace);
                    ResolverMgr.SetEndpointProperties("TargetNamespace", rowValue, resolution);
                }

                if (!string.IsNullOrEmpty(expr.TransformType))
                {
                    rowValue = ReadValueFromSso(expr.AffiliateAppName, expr.TransformType);
                    ResolverMgr.SetEndpointProperties("TransformType", rowValue, resolution);
                }

                if (!string.IsNullOrEmpty(expr.TransportLocation))
                {
                    rowValue = ReadValueFromSso(expr.AffiliateAppName, expr.TransportLocation);
                    ResolverMgr.SetEndpointProperties("TransportLocation", rowValue, resolution);
                }

                if (!string.IsNullOrEmpty(expr.TransportType))
                {
                    rowValue = ReadValueFromSso(expr.AffiliateAppName, expr.TransportType);
                    ResolverMgr.SetEndpointProperties("TransportType", rowValue, resolution);
                }

                ResolverMgr.SetResolverDictionary(resolution, resolverDictionary);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("BizTalk Server", exception.ToString(), EventLogEntryType.Error);
                throw;
            }

            return resolverDictionary;
        }

        /// <summary>
        /// Read a value from a particular SSO affiliate application.
        /// </summary>
        /// <param name="affiliateAppName">Name of the SSO affiliate application (usually .BTDFPROJ ProjectName value)</param>
        /// <param name="settingName">Name of the setting that corresponds to a row in the settings spreadsheet</param>
        /// <returns>Setting value</returns>
        private static string ReadValueFromSso(string affiliateAppName, string settingName)
        {
            string rowValue = SSOSettingsFileReader.ReadString(affiliateAppName, settingName);
            Debug.WriteLine(
                "Using SettingsFileGenerator value loaded into SSO affiliate app {0}: [{1}, {2}]", affiliateAppName, settingName, rowValue);
            return rowValue;
        }
    }
}
