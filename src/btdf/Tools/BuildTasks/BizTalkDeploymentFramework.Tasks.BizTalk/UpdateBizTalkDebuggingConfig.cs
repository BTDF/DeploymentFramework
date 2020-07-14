// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.XLANGs.BizTalk.CrossProcess;
using Microsoft.Build.Framework;

namespace DeploymentFramework.BuildTasks
{
    public class UpdateBizTalkDebuggingConfig : UpdateBTSNTSvcExeConfigBase
    {
        private string _validateAssemblies;
        private string _validateSchemas;
        private string _validateCorrelations;
        private string _extendedLogging;

        public string ValidateAssemblies
        {
            get { return _validateAssemblies; }
            set { _validateAssemblies = value; }
        }

        public string ValidateSchemas
        {
            get { return _validateSchemas; }
            set { _validateSchemas = value; }
        }

        public string ValidateCorrelations
        {
            get { return _validateCorrelations; }
            set { _validateCorrelations = value; }
        }

        public string ExtendedLogging
        {
            get { return _extendedLogging; }
            set { _extendedLogging = value; }
        }

        protected override bool UpdateConfiguration(Configuration config)
        {
            this.Log.LogMessage("Adding/updating debugging configuration data...");

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

            return true;
        }
    }
}
