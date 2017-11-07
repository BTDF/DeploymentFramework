using System;
using Genghis;
using Microsoft.RuleEngine;
using System.IO;
using Microsoft.BizTalk.RuleEngineExtensions;
using clp = Genghis.CommandLineParser;

namespace DeployBTRules
{
	/// <summary>
	/// This is a command line application for deploying BizTalk 2004 rules and vocabularies.
	/// </summary>
   class DeployRules
   {
      [clp.ParserUsage("Import and publish Policy/Vocabulary to the database from a file, and/or deploy & undeploy policy.")]
         class DeployRulesCommandLine : CommandLineParser
      {
         [clp.ValueUsage("Policy or Vocabulary File.",ValueName="ruleSetFile", Optional=true)]
         public string ruleSetFile = "";

         [clp.ValueUsage("Rule set name to operate on, if file contains policy.", ValueName="ruleSetName", Optional=true)]
         public string ruleSetName = "";

         [clp.FlagUsage("Undeploy the policy (default is to deploy)")]
         public bool undeploy = false;
      }

      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static int Main(string[] args)
      {
         DeployRulesCommandLine cl = new DeployRulesCommandLine();
         if( !cl.ParseAndContinue(args) ) 
            return -1;

         Microsoft.BizTalk.RuleEngineExtensions.RuleSetDeploymentDriver dd = 
            new Microsoft.BizTalk.RuleEngineExtensions.RuleSetDeploymentDriver();

         try
         {
            try
            {
               if(!cl.undeploy && cl.ruleSetFile != string.Empty)
                  dd.ImportAndPublishFileRuleStore(cl.ruleSetFile);
            }
            catch(System.Exception ex)
            {
               if(cl.ruleSetName != string.Empty)
               {
                  Console.WriteLine("Warning: Unable to import/publish {0} ({1}).\r\n\r\nAttempting deploy operation.",cl.ruleSetFile,ex.Message);
               }
               else
                  throw;
            }

            if(cl.ruleSetName != string.Empty)
            {
               RuleStore ruleStore = dd.GetRuleStore();
               RuleSetInfoCollection rsInfo = ruleStore.GetRuleSets(cl.ruleSetName, RuleStore.Filter.Latest);
               if (rsInfo.Count != 1)
               {
                  // oops ... error
                  throw new ApplicationException();
               }
               RuleSet ruleSet = ruleStore.GetRuleSet(rsInfo[0]);
               
               RuleSetInfo rsi = new RuleSetInfo(ruleSet.Name, 
                     ruleSet.CurrentVersion.MajorRevision, ruleSet.CurrentVersion.MinorRevision);

               if(cl.undeploy)
               {
                  dd.Undeploy(rsi);
               }
               else
               {
                  dd.Deploy(rsi);
               }

            }

            Console.WriteLine("\r\nDeployBTRules operation complete.");
         }
         catch(Microsoft.RuleEngine.RuleEngineDeploymentAlreadyDeployedException ex)
         {
            Console.WriteLine("\r\nDeployBTRules operation did not complete: " + ex.Message);
         }
         catch(RuleEngineDeploymentRuleSetExistsException ex)
         {
            Console.WriteLine("\r\nDeployBTRules operation did not complete: " + ex.Message);
         }
         catch(RuleEngineDeploymentVocabularyExistsException ex)
         {
            Console.WriteLine("\r\nDeployBTRules operation did not complete: " + ex.Message);
         }
         catch(System.Exception ex)
         {
            Console.WriteLine("failed: " + ex.ToString());
            return -1;
         }

         return 0;
      }

   

   }
}
