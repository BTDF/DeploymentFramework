// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace DeploymentFramework.BuildTasks
{
    public class SetRegistryValue : Task
	{
        private string _keyName;
		private string _valueName;
		private string _value;
        private RegistryValueKind _valueKind = RegistryValueKind.Unknown;

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

		[Required]
		public string Value
		{
			get { return _value; }
			set	{ _value = value; }
		}

        public string ValueKind
        {
            get { return _valueKind.ToString(); }
            set
            {
                if (!Enum.IsDefined(typeof(RegistryValueKind), value))
                {
                    throw new ArgumentException(
                        "ValueKind must be the string representation of one value from Microsoft.Win32.RegistryValueKind enumeration.");
                }

                _valueKind = (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), value, true);
            }
        }

        public override bool Execute()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_valueName))
                {
                    string[] parts = _keyName.Split('\\');
                    _valueName = parts[parts.Length - 1];
                    _keyName = _keyName.Remove(_keyName.Length - _valueName.Length - 1);
                }

                base.Log.LogMessage(MessageImportance.Normal, "SetRegistryValue: Attempting to create/update value '{0}' in registry value '{1}' at key '{2}'...", _value, _valueName, _keyName);
                Registry.SetValue(_keyName, _valueName, _value, _valueKind);
                base.Log.LogMessage(MessageImportance.Normal, "SetRegistryValue: Successfully created/updated value.");
            }
            catch (Exception ex)
            {
                base.Log.LogError("SetRegistryValue: Failed to create/update value '{0}' at key '{1}'.", _valueName, _keyName);
                base.Log.LogErrorFromException(ex, false);
                return false;
            }

            return true;
        }
    }
}
