// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE80;
using EnvDTE;

namespace DeploymentFramework.VisualStudioAddIn
{
    internal class CommandManager
    {
        internal const string DeployCommandName = "Deploy";
        internal const string UndeployCommandName = "Undeploy";
        internal const string DeployRulesCommandName = "DeployRules";
        internal const string UndeployRulesCommandName = "UndeployRules";
        internal const string ExportSettingsCommandName = "ExportSettings";
        internal const string ImportBindingsCommandName = "ImportBindings";
        internal const string PreprocessBindingsCommandName = "PreprocessBindings";
        internal const string BounceCommandName = "Bounce";
        internal const string UpdateSSOCommandName = "UpdateSSO";
        internal const string UpdateOrchsCommandName = "UpdateOrchs";
        internal const string BuildMSICommandName = "BuildMSI";
        internal const string GACProjectOutputCommandName = "GACProjectOutput";
        internal const string TerminateInstancesCommandName = "TerminateInstances";

        private const int DefaultCommandStatus = (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled;

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Commands2 _commands;

        internal CommandManager(DTE2 applicationObject, AddIn addInInstance)
        {
            this._applicationObject = applicationObject;
            this._commands = (Commands2)_applicationObject.Commands;
            this._addInInstance = addInInstance;
        }

        internal string GetFullCommandName(string commandName)
        {
            return _addInInstance.ProgID + "." + commandName;
        }

        internal void CreateCommands()
        {
            CreateCommand(
                DeployCommandName,
                "Deploy BizTalk Solution",
                "Deploys the BizTalk solution",
                133,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                UndeployCommandName,
                "Undeploy BizTalk Solution",
                "Undeploys the BizTalk solution",
                132,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                DeployRulesCommandName,
                "Deploy Rules and Vocabularies",
                "Deploys BizTalk Rules Engine vocabularies and rules",
                1,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStyleText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                UndeployRulesCommandName,
                "Undeploy Rules and Vocabularies",
                "Undeploys BizTalk Rules Engine vocabularies and rules",
                1,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStyleText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                ExportSettingsCommandName,
                "Export Environment Settings",
                "Exports settings from the spreadsheet to per-environment XML files",
                1104,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                ImportBindingsCommandName,
                "Import BizTalk Bindings",
                "Imports bindings from the PortBindings.xml file into the BizTalk application",
                1,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStyleText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                PreprocessBindingsCommandName,
                "Preprocess BizTalk Bindings",
                "Pre-processes bindings to create PortBindings.xml from MasterPortBindings.xml",
                1,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStyleText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                BounceCommandName,
                "Bounce BizTalk",
                "Restarts BizTalk hosts and IIS (if enabled)",
                37,  //3526  //1759
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                UpdateSSOCommandName,
                "Update SSO from Settings Spreadsheet",
                "Updates SSO with the current settings from the settings spreadsheet",
                1748,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                UpdateOrchsCommandName,
                "Quick Deploy BizTalk Solution",
                "Quickly updates orchestrations, components, transforms and SSO without a full deployment",
                136,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                BuildMSICommandName,
                "Build Server Deploy MSI",
                "Creates an MSI file for server deployment",
                1679, // 588,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                GACProjectOutputCommandName,
                "GAC Output of Selected Project",
                "Adds the binary output of the currently selected project to the GAC",
                1636,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);

            CreateCommand(
                TerminateInstancesCommandName,
                "Terminate All Service Instances",
                "Terminates all running or suspended service instances associated with the BizTalk application",
                1019,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton,
                null);
        }

        internal void RecreateCommands()
        {
            RecreateCommand(
                DeployCommandName,
                "Deploy BizTalk Solution",
                "Deploys the BizTalk solution",
                133,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                UndeployCommandName,
                "Undeploy BizTalk Solution",
                "Undeploys the BizTalk solution",
                132,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                DeployRulesCommandName,
                "Deploy Rules and Vocabularies",
                "Deploys BizTalk Rules Engine vocabularies and rules",
                1,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStyleText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                UndeployRulesCommandName,
                "Undeploy Rules and Vocabularies",
                "Undeploys BizTalk Rules Engine vocabularies and rules",
                1,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStyleText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                ExportSettingsCommandName,
                "Export Environment Settings",
                "Exports settings from the spreadsheet to per-environment XML files",
                1104,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                ImportBindingsCommandName,
                "Import BizTalk Bindings",
                "Imports bindings from the PortBindings.xml file into the BizTalk application",
                1,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStyleText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                PreprocessBindingsCommandName,
                "Preprocess BizTalk Bindings",
                "Pre-processes bindings to create PortBindings.xml from MasterPortBindings.xml",
                1,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStyleText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                BounceCommandName,
                "Bounce BizTalk",
                "Restarts BizTalk hosts and IIS (if enabled)",
                37,  //3526  //1759
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                UpdateSSOCommandName,
                "Update SSO from Settings Spreadsheet",
                "Updates SSO with the current settings from the settings spreadsheet",
                1748,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                UpdateOrchsCommandName,
                "Quick Deploy BizTalk Solution",
                "Quickly updates orchestrations, components, transforms and SSO without a full deployment",
                136,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                BuildMSICommandName,
                "Build Server Deploy MSI",
                "Creates an MSI file for server deployment",
                1679, // 588,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                GACProjectOutputCommandName,
                "GAC Output of Selected Project",
                "Adds the binary output of the currently selected project to the GAC",
                1636,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);

            RecreateCommand(
                TerminateInstancesCommandName,
                "Terminate All Service Instances",
                "Terminates all running or suspended service instances associated with the BizTalk application",
                1019,
                DefaultCommandStatus,
                (int)vsCommandStyle.vsCommandStylePictAndText,
                vsCommandControlType.vsCommandControlTypeButton);
        }

        private void CreateCommand(
            string name, string buttonText, string tooltip, object bitmap, int commandStatusValue, int commandStyleFlags,
            vsCommandControlType controlType, object bindings)
        {
            Command cmd = null;
            object[] contextGUIDS = new object[] { };

            try
            {
                cmd = _commands.AddNamedCommand2(
                    _addInInstance,
                    name,
                    buttonText,
                    tooltip,
                    true,
                    bitmap,
                    ref contextGUIDS,
                    commandStatusValue,
                    commandStyleFlags,
                    controlType);

                if (bindings != null)
                {
                    cmd.Bindings = bindings;
                }
            }
            catch (Exception)
            {
            }
        }

        private void RecreateCommand(
            string name, string buttonText, string tooltip, object bitmap, int commandStatusValue, int commandStyleFlags,
            vsCommandControlType controlType)
        {
            Command existingCommand = null;
            object bindings = null;

            // Try to get the command if it exists to get the current binding
            try
            {
                existingCommand = _applicationObject.Commands.Item(GetFullCommandName(name), -1);
            }
            catch
            {
            }

            if (existingCommand == null)
            {
                // This should not happen!
            }
            else
            {
                // We must preserve the command bindings
                bindings = existingCommand.Bindings;

                // Remove the existing command
                existingCommand.Delete();

                // Create it again
                CreateCommand(name, buttonText, tooltip, bitmap, commandStatusValue, commandStyleFlags, controlType, bindings);
            }
        }
    }
}
