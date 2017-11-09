// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.XLANGs.BizTalk.CrossProcess;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DeploymentFramework.BuildTasks
{
    /// <summary>
    /// This task can update BizTalk's BTSNTSvc.exe.config to specify a non-default AppDomain name
    /// and exact and/or pattern-based matching of BizTalk assemblies to be loaded into that AppDomain.
    /// At runtime, BizTalk normally creates default AppDomains and mixes assemblies from multiple
    /// BizTalk applications into the same AppDomain.  If an application uses singleton or static objects,
    /// their state will be shared by the entire AppDomain, which may include unrelated applications.
    /// To isolate an application from all others, use this task to create an isolated AppDomain for
    /// a particular application's assemblies.
    /// </summary>
    public class UpdateBizTalkAppDomainConfig : UpdateBTSNTSvcExeConfigBase
    {
        private string _appDomainName;
        private string _defaultAssembliesPerDomain;
        private ITaskItem[] _patternAssignmentRules;
        private ITaskItem[] _appDomains;
        private string _defaultAssemblyNameRegexes;
        private string _remove = "false";

        /// <summary>
        /// Name of the custom AppDomain
        /// </summary>
        [Obsolete]
        public string AppDomainName
        {
            get { return _appDomainName; }
            set { _appDomainName = value; }
        }

        /// <summary>
        /// Default number of assemblies that BizTalk will load into a single AppDomain.
        /// </summary>
        public string DefaultAssembliesPerDomain
        {
            get { return _defaultAssembliesPerDomain; }
            set { _defaultAssembliesPerDomain = value; }
        }

        /// <summary>
        /// Regular expressions of assembly names for those assemblies to be loaded into this AppDomain.
        /// </summary>
        [Obsolete]
        public ITaskItem[] PatternAssignmentRules
        {
            get { return _patternAssignmentRules; }
            set { _patternAssignmentRules = value; }
        }

        public ITaskItem[] AppDomains
        {
            get { return _appDomains; }
            set { _appDomains = value; }
        }

        /// <summary>
        /// Default pattern assignment regular expressions if no pattern or exact rules were provided
        /// </summary>
        public string DefaultAssemblyNameRegexes
        {
            get { return _defaultAssemblyNameRegexes; }
            set { _defaultAssemblyNameRegexes = value; }
        }

        /// <summary>
        /// Indictates whether the specified AppDomain configuration should be removed.
        /// </summary>
        public string Remove
        {
            get { return _remove; }
            set { _remove = value; }
        }

        /// <summary>
        /// Override the BizTalk xlangs configuration data loaded from the BTSNTSvc.exe.config file.
        /// </summary>
        /// <param name="config"></param>
        protected override bool UpdateConfiguration(Configuration config)
        {
            UpdateDefaultAppDomainConfiguration(config);

            bool remove = bool.Parse(_remove);

            // If no AppDomainName AND no AppDomains, there is nothing to do
            if (string.IsNullOrWhiteSpace(_appDomainName) && (_appDomains == null || _appDomains.Length == 0))
            {
                return true;
            }

            // If there is an AppDomainName but no AppDomains, then convert it into the AppDomains list
            if (!string.IsNullOrWhiteSpace(_appDomainName) && (_appDomains == null || _appDomains.Length == 0))
            {
                ConvertPropertiesToAppDomainsList();
            }

            List<AppDomainSpec> adss = config.AppDomains.AppDomainSpecs.ToList();

            foreach (ITaskItem appDomain in _appDomains)
            {
                AppDomainSpec ads = adss.FirstOrDefault(adsx => (string.Compare(adsx.Name, appDomain.ItemSpec, true) == 0));

                if (remove)
                {
                    this.Log.LogMessage("Removing AppDomain configuration data for AppDomain '{0}'...", appDomain.ItemSpec);

                    if (ads != null)
                    {
                        adss.Remove(ads);
                    }

                    RemoveAssignmentRules(config.AppDomains, appDomain.ItemSpec);
                }
                else
                {
                    this.Log.LogMessage("Adding/updating AppDomain configuration data for AppDomain '{0}'...", appDomain.ItemSpec);

                    if (ads == null)
                    {
                        ads = new AppDomainSpec();
                        ads.Name = appDomain.ItemSpec;
                        adss.Add(ads);
                    }

                    if (!string.IsNullOrWhiteSpace(appDomain.GetMetadata("SecondsIdleBeforeShutdown")))
                    {
                        ads.SecondsIdleBeforeShutdown = int.Parse(appDomain.GetMetadata("SecondsIdleBeforeShutdown"));
                    }

                    if (!string.IsNullOrWhiteSpace(appDomain.GetMetadata("SecondsEmptyBeforeShutdown")))
                    {
                        ads.SecondsEmptyBeforeShutdown = int.Parse(appDomain.GetMetadata("SecondsEmptyBeforeShutdown"));
                    }

                    string configFilePath = appDomain.GetMetadata("ConfigurationFilePath");
                    if (!string.IsNullOrWhiteSpace(configFilePath))
                    {
                        configFilePath = System.IO.Path.GetFullPath(configFilePath);

                        if (!System.IO.File.Exists(configFilePath))
                        {
                            this.Log.LogError("The file in BizTalkAppDomain ConfigurationFilePath cannot be found. [" + configFilePath + "]");
                            return false;
                        }

                        ads.BaseSetup.ConfigurationFile = configFilePath;
                    }
                    else
                    {
                        ads.BaseSetup.ConfigurationFile = null;
                    }

                    string applicationBase = appDomain.GetMetadata("ApplicationBase");
                    if (!string.IsNullOrWhiteSpace(applicationBase))
                    {
                        if (!applicationBase.EndsWith("\\"))
                        {
                            applicationBase += "\\";
                        }

                        applicationBase = System.IO.Path.GetFullPath(applicationBase);

                        ads.BaseSetup.ApplicationBase = applicationBase;
                    }
                    else
                    {
                        ads.BaseSetup.ApplicationBase = null;
                    }

                    string privateBinPath = appDomain.GetMetadata("PrivateBinPath");
                    if (!string.IsNullOrWhiteSpace(privateBinPath))
                    {
                        string[] privateBinPaths = privateBinPath.Split(';');

                        foreach (string privateBinPathInstance in privateBinPaths)
                        {
                            if (System.IO.Path.IsPathRooted(privateBinPathInstance))
                            {
                                this.Log.LogError("The path(s) in BizTalkAppDomain PrivateBinPath must be relative to ApplicationBase. [" + privateBinPathInstance + "]");
                                return false;
                            }
                        }

                        ads.BaseSetup.PrivateBinPath = privateBinPath;
                    }
                    else
                    {
                        ads.BaseSetup.PrivateBinPath = null;
                    }

                    UpdateAssignmentRules(config.AppDomains, appDomain.ItemSpec, appDomain.GetMetadata("AssemblyNameRegexes"), appDomain.GetMetadata("AssemblyNames"));
                }
            }

            config.AppDomains.AppDomainSpecs = adss.ToArray();

            return true;
        }

        private void ConvertPropertiesToAppDomainsList()
        {
            ITaskItem newAppDomain = new TaskItem(_appDomainName);

            if (_patternAssignmentRules != null)
            {
                List<string> patternAssignmentRules = new List<string>();

                foreach (ITaskItem patternAssignmentRule in _patternAssignmentRules)
                {
                    string rule = patternAssignmentRule.GetMetadata("AssemblyNamePattern");

                    if (!string.IsNullOrWhiteSpace(rule))
                    {
                        patternAssignmentRules.Add(rule);
                    }
                }

                if (patternAssignmentRules.Count > 0)
                {
                    newAppDomain.SetMetadata("AssemblyNameRegexes", string.Join(";", patternAssignmentRules));
                }
            }

            _appDomains = new ITaskItem[] { newAppDomain };
            _appDomainName = null;
            _patternAssignmentRules = null;
        }

        private void UpdateDefaultAppDomainConfiguration(Configuration config)
        {
            if (!string.IsNullOrEmpty(_defaultAssembliesPerDomain))
            {
                config.AppDomains.AssembliesPerDomain = int.Parse(_defaultAssembliesPerDomain);
            }
        }

        /// <summary>
        /// Update the assignment rules section for the current AppDomain.
        /// </summary>
        private void UpdateAssignmentRules(AppDomains adConfig, string appDomainName, string assemblyNameRegexes, string assemblyNames)
        {
            if (string.IsNullOrWhiteSpace(assemblyNameRegexes) && string.IsNullOrWhiteSpace(assemblyNames))
            {
                assemblyNameRegexes = _defaultAssemblyNameRegexes;
            }

            RemoveAssignmentRules(adConfig, appDomainName);
            UpdateExactAssignmentRules(adConfig, appDomainName, assemblyNames);
            UpdatePatternAssignmentRules(adConfig, appDomainName, assemblyNameRegexes);
        }

        private void RemoveAssignmentRules(AppDomains adConfig, string appDomainName)
        {
            RemoveExactAssignmentRules(adConfig, appDomainName);
            RemovePatternAssignmentRules(adConfig, appDomainName);
        }

        /// <summary>
        /// Update the ExactAssignmentRules section for the current AppDomain.
        /// </summary>
        /// <param name="adConfig"></param>
        private void UpdateExactAssignmentRules(AppDomains adConfig, string appDomainName, string assemblyNames)
        {
            if (string.IsNullOrWhiteSpace(assemblyNames))
            {
                return;
            }

            List<ExactAssignmentRule> ears = adConfig.ExactAssignmentRules.ToList();

            string[] assemblyNamesSplit = assemblyNames.Split(';');

            foreach (string assemblyName in assemblyNamesSplit)
            {
                ExactAssignmentRule ear = FindMatchOnAssemblyAppDomain(ears, assemblyName, appDomainName);

                if (ear == null)
                {
                    ear = new ExactAssignmentRule();
                    ear.AssemblyName = assemblyName;
                    ear.AppDomainName = appDomainName;
                    ears.Add(ear);
                }
            }

            adConfig.ExactAssignmentRules = ears.ToArray();
        }

        private void RemoveExactAssignmentRules(AppDomains adConfig, string appDomainName)
        {
            List<ExactAssignmentRule> ears = adConfig.ExactAssignmentRules.ToList();

            ears.RemoveAll(ear => (string.Compare(ear.AppDomainName, appDomainName, true) == 0));

            adConfig.ExactAssignmentRules = ears.ToArray();
        }

        /// <summary>
        /// Update the PatternAssignmentRules section for the current AppDomain.
        /// </summary>
        /// <param name="adConfig"></param>
        private void UpdatePatternAssignmentRules(AppDomains adConfig, string appDomainName, string assemblyNameRegexes)
        {
            if (string.IsNullOrWhiteSpace(assemblyNameRegexes))
            {
                return;
            }

            List<PatternAssignmentRule> pars = adConfig.PatternAssignmentRules.ToList();

            string[] assemblyNameRegexesSplit = assemblyNameRegexes.Split(';');

            foreach (string assemblyNameRegex in assemblyNameRegexesSplit)
            {
                PatternAssignmentRule par = FindMatchOnAssemblyPatternAppDomain(pars, assemblyNameRegex, appDomainName);

                if (par == null)
                {
                    par = new PatternAssignmentRule();
                    par.AssemblyNamePattern = assemblyNameRegex;
                    par.AppDomainName = appDomainName;
                    pars.Add(par);
                }
            }

            adConfig.PatternAssignmentRules = pars.ToArray();
        }

        private void RemovePatternAssignmentRules(AppDomains adConfig, string appDomainName)
        {
            List<PatternAssignmentRule> pars = adConfig.PatternAssignmentRules.ToList();

            pars.RemoveAll(par => (string.Compare(par.AppDomainName, appDomainName, true) == 0));

            adConfig.PatternAssignmentRules = pars.ToArray();
        }

        #region Array Search Helpers
        private ExactAssignmentRule FindMatchOnAssemblyAppDomain(
            List<ExactAssignmentRule> ears, string assemblyName, string appDomainName)
        {
            foreach (ExactAssignmentRule ear in ears)
            {
                if (string.Compare(ear.AppDomainName, appDomainName, true) == 0
                    && string.Compare(ear.AssemblyName, assemblyName, true) == 0)
                {
                    return ear;
                }
            }

            return null;
        }


        private PatternAssignmentRule FindMatchOnAssemblyPatternAppDomain(
            List<PatternAssignmentRule> pars, string assemblyNamePattern, string appDomainName)
        {
            foreach (PatternAssignmentRule par in pars)
            {
                if (string.Compare(par.AppDomainName, appDomainName, true) == 0
                    && string.Compare(par.AssemblyNamePattern, assemblyNamePattern, true) == 0)
                {
                    return par;
                }
            }

            return null;
        }
        #endregion
    }
}
