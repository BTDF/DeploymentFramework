// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using Microsoft.RuleEngine;

namespace DeployBTRules
{
    /// <summary>
    /// This is a command line application for deploying BizTalk rules and vocabularies.
    /// </summary>
    class DeployRules
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            DeployRulesCommandLine cl = new DeployRulesCommandLine();
            if (!cl.ParseAndContinue(args))
            {
                return -1;
            }

            if (string.IsNullOrEmpty(cl.ruleSetFile) && string.IsNullOrEmpty(cl.ruleSetName) && string.IsNullOrEmpty(cl.vocabularyName))
            {
                Console.WriteLine(cl.GetUsage());
                return -1;
            }

            if (!string.IsNullOrEmpty(cl.ruleSetName) && !string.IsNullOrEmpty(cl.vocabularyName))
            {
                Console.WriteLine(cl.GetUsage());
                return -1;
            }

            if (cl.unpublish)
            {
                // If we're unpublishing then we must also undeploy.
                cl.undeploy = true;
            }

            cl.PrintLogo();

            Microsoft.BizTalk.RuleEngineExtensions.RuleSetDeploymentDriver dd =
               new Microsoft.BizTalk.RuleEngineExtensions.RuleSetDeploymentDriver();

            try
            {
                try
                {
                    if (!cl.undeploy && cl.ruleSetFile != string.Empty)
                    {
                        Console.WriteLine("Importing and publishing from file '{0}'...", cl.ruleSetFile);
                        dd.ImportAndPublishFileRuleStore(cl.ruleSetFile);
                    }
                }
                catch (System.Exception ex)
                {
                    if (cl.ruleSetName != string.Empty)
                    {
                        Console.WriteLine("Unable to import/publish {0} ({1}). Attempting deploy operation.", cl.ruleSetFile, ex.Message);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (!string.IsNullOrEmpty(cl.ruleSetName))
                {
                    ProcessPolicies(cl, dd);
                }

                if (!string.IsNullOrEmpty(cl.vocabularyName) && cl.unpublish)
                {
                    ProcessVocabularies(cl, dd);
                }

                Console.WriteLine("Operation complete.");
            }
            catch (Microsoft.RuleEngine.RuleEngineDeploymentAlreadyDeployedException ex)
            {
                Console.WriteLine("Operation did not complete: " + ex.Message);
            }
            catch (Microsoft.RuleEngine.RuleEngineDeploymentNotDeployedException ex)
            {
                Console.WriteLine("Operation did not complete: " + ex.Message);
            }
            catch (RuleEngineDeploymentRuleSetExistsException ex)
            {
                Console.WriteLine("Operation did not complete: " + ex.Message);
            }
            catch (RuleEngineDeploymentVocabularyExistsException ex)
            {
                Console.WriteLine("Operation did not complete: " + ex.Message);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Failed: " + ex.ToString());
                Console.WriteLine();
                return -1;
            }

            Console.WriteLine();
            return 0;
        }

        private static void ProcessPolicies(
            DeployRulesCommandLine cl, Microsoft.BizTalk.RuleEngineExtensions.RuleSetDeploymentDriver dd)
        {
            RuleStore ruleStore = dd.GetRuleStore();
            RuleSetInfoCollection rsInfo = ruleStore.GetRuleSets(cl.ruleSetName, RuleStore.Filter.All);

            Version version = ParseVersion(cl.ruleSetVersion);

            RuleSetInfo matchingRuleSetInfo = null;

            foreach (RuleSetInfo currentRsi in rsInfo)
            {
                if (currentRsi.MajorRevision == version.Major
                    && currentRsi.MinorRevision == version.Minor)
                {
                    matchingRuleSetInfo = currentRsi;
                    break;
                }
            }

            if (matchingRuleSetInfo == null)
            {
                Console.WriteLine(
                    "No published ruleset with name '" + cl.ruleSetName + "' and version '" + cl.ruleSetVersion + "'.");
            }
            else if (cl.undeploy)
            {
                Console.WriteLine("Undeploying rule set '{0}' version {1}.{2}...", cl.ruleSetName, version.Major, version.Minor);

                if (dd.IsRuleSetDeployed(matchingRuleSetInfo))
                {
                    dd.Undeploy(matchingRuleSetInfo);
                }
                else
                {
                    Console.WriteLine("  Rule set is not currently deployed.");
                }

                if (cl.unpublish)
                {
                    Console.WriteLine("Unpublishing rule set '{0}' version {1}.{2}...", cl.ruleSetName, version.Major, version.Minor);
                    ruleStore.Remove(matchingRuleSetInfo);
                }
            }
            else
            {
                Console.WriteLine("Deploying rule set '{0}' version {1}.{2}...", cl.ruleSetName, version.Major, version.Minor);
                dd.Deploy(matchingRuleSetInfo);
            }
        }

        private static void ProcessVocabularies(
            DeployRulesCommandLine cl, Microsoft.BizTalk.RuleEngineExtensions.RuleSetDeploymentDriver dd)
        {
            RuleStore ruleStore = dd.GetRuleStore();
            VocabularyInfoCollection vInfo = ruleStore.GetVocabularies(cl.vocabularyName, RuleStore.Filter.All);

            Version version = ParseVersion(cl.ruleSetVersion);

            VocabularyInfo matchingVocabularyInfo = null;

            foreach (VocabularyInfo currentRsi in vInfo)
            {
                if (currentRsi.MajorRevision == version.Major
                    && currentRsi.MinorRevision == version.Minor)
                {
                    matchingVocabularyInfo = currentRsi;
                    break;
                }
            }

            if (matchingVocabularyInfo == null)
            {
                Console.WriteLine(
                    "No published vocabulary with name '" + cl.vocabularyName + "' and version '" + cl.ruleSetVersion + "'.");
            }
            else if (cl.unpublish)
            {
                Console.WriteLine("Unpublishing vocabulary '{0}' version {1}.{2}...", cl.vocabularyName, version.Major, version.Minor);
                ruleStore.Remove(matchingVocabularyInfo);
            }
        }

        private static Version ParseVersion(string versionString)
        {
            string[] parts = versionString.Split('.');

            if (parts == null || parts.Length != 2)
            {
                throw new ArgumentException("Invalid version number. Must be in the format Major.Minor");
            }

            int majorVersion = 0;
            int minorVersion = 0;

            bool majorIsOk = int.TryParse(parts[0], out majorVersion);
            bool minorIsOk = int.TryParse(parts[1], out minorVersion);

            if (!majorIsOk || !minorIsOk)
            {
                throw new ArgumentException("Invalid version number. Must be in the format Major.Minor");
            }

            return new Version(majorVersion, minorVersion);
        }
    }
}
