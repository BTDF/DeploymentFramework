// By Thomas F. Abraham

using System;
using System.Collections.Generic;
using NAnt.Core.Attributes;
using Microsoft.XLANGs.BizTalk.CrossProcess;
using NAnt.Core;

namespace BizTalk.NAnt.Tasks
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
    [TaskName("updateBizTalkAppDomainConfig")]
    public class UpdateBizTalkAppDomainConfig : UpdateBTSNTSvcExeConfigBase
    {
        private string _appDomainName;
        private string _secondsIdleBeforeShutdown;
        private string _secondsEmptyBeforeShutdown;
        private string _configurationFilePath;
        private string _defaultAssembliesPerDomain;
        private AdExactAssignmentRules _exactAssignmentRules;
        private AdPatternAssignmentRules _patternAssignmentRules;
        private string _remove = "false";

        /// <summary>
        /// Name of the custom AppDomain
        /// </summary>
        [TaskAttribute("appDomainName")]
        public string AppDomainName
        {
            get { return _appDomainName; }
            set { _appDomainName = value; }
        }

        /// <summary>
        /// Number of seconds that the AppDomain is idle (contains only dehydratable orchestrations) before being unloaded.
        /// </summary>
        [TaskAttribute("secondsIdleBeforeShutdown")]
        [Int32Validator]
        public string SecondsIdleBeforeShutdown
        {
            get { return _secondsIdleBeforeShutdown; }
            set { _secondsIdleBeforeShutdown = value; }
        }

        /// <summary>
        /// Number of seconds that the AppDomain is empty (does not contain any orchestrations) before being unloaded.
        /// </summary>
        [TaskAttribute("secondsEmptyBeforeShutdown")]
        [Int32Validator]
        public string SecondsEmptyBeforeShutdown
        {
            get { return _secondsEmptyBeforeShutdown; }
            set { _secondsEmptyBeforeShutdown = value; }
        }

        /// <summary>
        /// Complete path to a standard .NET configuration file to be loaded into the AppDomain.
        /// </summary>
        [TaskAttribute("configurationFilePath")]
        [StringValidator]
        public string ConfigurationFilePath
        {
            get { return _configurationFilePath; }
            set { _configurationFilePath = value; }
        }

        /// <summary>
        /// Default number of assemblies that BizTalk will load into a single AppDomain.
        /// </summary>
        [TaskAttribute("defaultAssembliesPerDomain")]
        [Int32Validator]
        public string DefaultAssembliesPerDomain
        {
            get { return _defaultAssembliesPerDomain; }
            set { _defaultAssembliesPerDomain = value; }
        }

        /// <summary>
        /// Exact assembly names for those assemblies to be loaded into this AppDomain.
        /// </summary>
        [BuildElement("exactAssignmentRules")]
        public AdExactAssignmentRules ExactAssignmentRules
        {
            get { return _exactAssignmentRules; }
            set { _exactAssignmentRules = value; }
        }

        /// <summary>
        /// Regular expressions of assembly names for those assemblies to be loaded into this AppDomain.
        /// </summary>
        [BuildElement("patternAssignmentRules")]
        public AdPatternAssignmentRules PatternAssignmentRules
        {
            get { return _patternAssignmentRules; }
            set { _patternAssignmentRules = value; }
        }

        /// <summary>
        /// Indictates whether the specified AppDomain configuration should be removed.
        /// </summary>
        [TaskAttribute("remove")]
        [BooleanValidator]
        public string Remove
        {
            get { return _remove; }
            set { _remove = value; }
        }
	
        /// <summary>
        /// Override the BizTalk xlangs configuration data loaded from the BTSNTSvc.exe.config file.
        /// </summary>
        /// <param name="config"></param>
        protected override void UpdateConfiguration(Configuration config)
        {
            UpdateDefaultAppDomainConfiguration(config);

            bool remove = bool.Parse(_remove);

            if (!string.IsNullOrEmpty(_appDomainName))
            {
                List<AppDomainSpec> adss = new List<AppDomainSpec>(config.AppDomains.AppDomainSpecs);

                AppDomainSpec ads = adss.Find(AppDomainSpecNameMatchesAppDomainName);

                if (remove)
                {
                    this.Log(Level.Info, "Removing AppDomain configuration data for AppDomain '" + _appDomainName + "'...");

                    if (ads != null)
                    {
                        adss.Remove(ads);
                    }

                    RemoveExactAssignmentRules(config.AppDomains);
                    RemovePatternAssignmentRules(config.AppDomains);
                }
                else
                {
                    this.Log(Level.Info, "Adding/updating AppDomain configuration data for AppDomain '" + _appDomainName + "'...");

                    if (ads == null)
                    {
                        ads = new AppDomainSpec();
                        ads.Name = _appDomainName;
                        adss.Add(ads);
                    }

                    if (!string.IsNullOrEmpty(_secondsIdleBeforeShutdown))
                    {
                        ads.SecondsIdleBeforeShutdown = int.Parse(_secondsIdleBeforeShutdown);
                    }

                    if (!string.IsNullOrEmpty(_secondsEmptyBeforeShutdown))
                    {
                        ads.SecondsEmptyBeforeShutdown = int.Parse(_secondsEmptyBeforeShutdown);
                    }

                    if (!string.IsNullOrEmpty(_configurationFilePath))
                    {
                        ads.BaseSetup.ConfigurationFile = _configurationFilePath;
                    }

                    UpdateExactAssignmentRules(config.AppDomains);
                    UpdatePatternAssignmentRules(config.AppDomains);
                }

                config.AppDomains.AppDomainSpecs = adss.ToArray();
            }
        }

        private void UpdateDefaultAppDomainConfiguration(Configuration config)
        {
            if (!string.IsNullOrEmpty(_defaultAssembliesPerDomain))
            {
                config.AppDomains.AssembliesPerDomain = int.Parse(_defaultAssembliesPerDomain);
            }
        }

        /// <summary>
        /// Update the ExactAssignmentRules section for the current AppDomain.
        /// </summary>
        /// <param name="adConfig"></param>
        private void UpdateExactAssignmentRules(AppDomains adConfig)
        {
            if (_exactAssignmentRules == null)
            {
                return;
            }

            List<ExactAssignmentRule> ears = new List<ExactAssignmentRule>(adConfig.ExactAssignmentRules);

            foreach (AdExactAssignmentRule adear in _exactAssignmentRules.ExactAssignments)
            {
                ExactAssignmentRule ear = FindMatchOnAssemblyAppDomain(ears, adear.AssemblyName, _appDomainName);

                if (ear == null)
                {
                    ear = new ExactAssignmentRule();
                    ear.AssemblyName = adear.AssemblyName;
                    ear.AppDomainName = _appDomainName;
                    ears.Add(ear);
                }

                adConfig.ExactAssignmentRules = ears.ToArray();
            }
        }

        private void RemoveExactAssignmentRules(AppDomains adConfig)
        {
            ExactAssignmentRule ear = null;
            List<ExactAssignmentRule> ears = new List<ExactAssignmentRule>(adConfig.ExactAssignmentRules);

            do
            {
                ear = ears.Find(ExactAssignmentAppDomainNameMatchesAppDomainName);

                if (ear != null)
                {
                    ears.Remove(ear);
                }
            }
            while (ear != null);

            adConfig.ExactAssignmentRules = ears.ToArray();
        }

        /// <summary>
        /// Update the PatternAssignmentRules section for the current AppDomain.
        /// </summary>
        /// <param name="adConfig"></param>
        private void UpdatePatternAssignmentRules(AppDomains adConfig)
        {
            if (_patternAssignmentRules == null)
            {
                return;
            }

            List<PatternAssignmentRule> pars = new List<PatternAssignmentRule>(adConfig.PatternAssignmentRules);

            foreach (AdPatternAssignmentRule adpar in _patternAssignmentRules.PatternAssignments)
            {
                PatternAssignmentRule par = FindMatchOnAssemblyPatternAppDomain(pars, adpar.AssemblyNamePattern, _appDomainName);

                if (par == null)
                {
                    par = new PatternAssignmentRule();
                    par.AssemblyNamePattern = adpar.AssemblyNamePattern;
                    par.AppDomainName = _appDomainName;
                    pars.Add(par);
                }

                adConfig.PatternAssignmentRules = pars.ToArray();
            }
        }

        private void RemovePatternAssignmentRules(AppDomains adConfig)
        {
            PatternAssignmentRule par = null;
            List<PatternAssignmentRule> pars = new List<PatternAssignmentRule>(adConfig.PatternAssignmentRules);

            do
            {
                par = pars.Find(PatternAssignmentAppDomainNameMatchesAppDomainName);

                if (par != null)
                {
                    pars.Remove(par);
                }
            }
            while (par != null);

            adConfig.PatternAssignmentRules = pars.ToArray();
        }

        #region Array Search Helpers
        /// <summary>
        /// Find an AppDomainSpec with a name matching the current AppDomain name.
        /// </summary>
        /// <param name="ads"></param>
        /// <returns></returns>
        private bool AppDomainSpecNameMatchesAppDomainName(AppDomainSpec ads)
        {
            if (string.Compare(ads.Name, _appDomainName, true) == 0)
            {
                return true;
            }

            return false;
        }

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

        /// <summary>
        /// Find an ExactAssignmentRule with an AppDomainName matching the current AppDomain name.
        /// </summary>
        /// <param name="ads"></param>
        /// <returns></returns>
        private bool ExactAssignmentAppDomainNameMatchesAppDomainName(ExactAssignmentRule ear)
        {
            if (string.Compare(ear.AppDomainName, _appDomainName, true) == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find an PatternAssignmentRule with an AppDomainName matching the current AppDomain name.
        /// </summary>
        /// <param name="ads"></param>
        /// <returns></returns>
        private bool PatternAssignmentAppDomainNameMatchesAppDomainName(PatternAssignmentRule par)
        {
            if (string.Compare(par.AppDomainName, _appDomainName, true) == 0)
            {
                return true;
            }

            return false;
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

    public class AdExactAssignmentRules : Element
    {
        private AdExactAssignmentRule[] _exactAssignmentRule;

        [BuildElementArray("exactAssignmentRule")]
        public AdExactAssignmentRule[] ExactAssignments
        {
            get { return _exactAssignmentRule; }
            set { _exactAssignmentRule = value; }
        }
    }

    public class AdPatternAssignmentRules : Element
    {
        private AdPatternAssignmentRule[] _patternAssignmentRule;

        [BuildElementArray("patternAssignmentRule")]
        public AdPatternAssignmentRule[] PatternAssignments
        {
            get { return _patternAssignmentRule; }
            set { _patternAssignmentRule = value; }
        }
    }

    public class AdExactAssignmentRule : Element
    {
        private string _assemblyName;

        [TaskAttribute("assemblyName")]
        public string AssemblyName
        {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }
    }

    public class AdPatternAssignmentRule : Element
    {
        private string _assemblyNamePattern;

        [TaskAttribute("assemblyNamePattern")]
        public string AssemblyNamePattern
        {
            get { return _assemblyNamePattern; }
            set { _assemblyNamePattern = value; }
        }
    }
}
