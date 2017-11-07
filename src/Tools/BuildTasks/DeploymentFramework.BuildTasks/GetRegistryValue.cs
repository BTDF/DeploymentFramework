// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace DeploymentFramework.BuildTasks
{
    public class GetRegistryValue : Task
	{
        private string _keyName;
		private string _valueName;
		private string _value;
        private string _defaultValue = string.Empty;

        [Required]
        public string KeyName
        {
            get { return _keyName; }
            set { _keyName = value; }
        }

		[Required]
		public string ValueName
		{
			get { return _valueName; }
			set	{ _valueName = value; }
		}

        [Output]
		public string Value
		{
			get { return _value; }
			set	{ _value = value; }
		}

        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        public override bool Execute()
        {
            try
            {
                base.Log.LogMessage(MessageImportance.Low, "GetRegistryValue: Attempting to get registry value '{0}' at key '{1}'...", _valueName, _keyName);
                
                object regValue = Registry.GetValue(_keyName, _valueName, _defaultValue);

                if (regValue == null)
                {
                    _value = _defaultValue;
                }
                else
                {
                    _value = regValue.ToString();
                }

                base.Log.LogMessage(MessageImportance.Low, "GetRegistryValue: Successfully got registry value.");
            }
            catch (Exception ex)
            {
                base.Log.LogError("GetRegistryValue: Failed to get registry value '{0}' at key '{1}'.", _valueName, _keyName);
                base.Log.LogErrorFromException(ex, false);
                return false;
            }

            return true;
        }
    }
}
