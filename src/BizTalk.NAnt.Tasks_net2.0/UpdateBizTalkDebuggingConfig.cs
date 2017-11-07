using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.XLANGs.BizTalk.CrossProcess;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace BizTalk.NAnt.Tasks
{
    [TaskName("updateBizTalkDebuggingConfig")]
    public class UpdateBizTalkDebuggingConfig : UpdateBTSNTSvcExeConfigBase
    {
        private string _validateAssemblies;
        private string _validateSchemas;
        private string _validateCorrelations;
        private string _extendedLogging;

        [TaskAttribute("validateAssemblies")]
        [BooleanValidator]
        public string ValidateAssemblies
        {
            get { return _validateAssemblies; }
            set { _validateAssemblies = value; }
        }

        [TaskAttribute("validateSchemas")]
        [BooleanValidator]
        public string ValidateSchemas
        {
            get { return _validateSchemas; }
            set { _validateSchemas = value; }
        }

        [TaskAttribute("validateCorrelations")]
        [BooleanValidator]
        public string ValidateCorrelations
        {
            get { return _validateCorrelations; }
            set { _validateCorrelations = value; }
        }

        [TaskAttribute("extendedLogging")]
        [BooleanValidator]
        public string ExtendedLogging
        {
            get { return _extendedLogging; }
            set { _extendedLogging = value; }
        }

        protected override void UpdateConfiguration(Configuration config)
        {
            this.Log(Level.Info, "Adding/updating debugging configuration data...");

            if (!string.IsNullOrEmpty(_validateAssemblies))
            {
                config.Debugging.ValidateAssemblies = bool.Parse(_validateAssemblies);
            }

            if (!string.IsNullOrEmpty(_validateCorrelations))
            {
                config.Debugging.ValidateCorrelations = bool.Parse(_validateCorrelations);
            }

            if (!string.IsNullOrEmpty(_validateSchemas))
            {
                config.Debugging.ValidateSchemas = bool.Parse(_validateSchemas);
            }

            if (!string.IsNullOrEmpty(_extendedLogging))
            {
                config.Debugging.ExtendedLogging = bool.Parse(_extendedLogging);
            }
        }
    }
}
