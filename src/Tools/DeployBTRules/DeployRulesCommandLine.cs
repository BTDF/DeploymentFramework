// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Genghis;
using clp = Genghis.CommandLineParser;

namespace DeployBTRules
{
    [clp.ParserUsage("Import and publish BizTalk Policy/Vocabulary to the database from a file, and/or deploy & undeploy policy.")]
    class DeployRulesCommandLine : CommandLineParser
    {
        [clp.ValueUsage("Policy or Vocabulary File.", ValueName = "ruleSetFile", Optional = true)]
        public string ruleSetFile = "";

        [clp.ValueUsage("Rule set name to operate on, if file contains policy.", ValueName = "ruleSetName", Optional = true)]
        public string ruleSetName = "";

        [clp.ValueUsage("Vocabulary name to operate on, if file contains vocabularies.", ValueName = "vocabularyName", Optional = true)]
        public string vocabularyName = "";

        [clp.FlagUsage("Undeploy the policy (default is to deploy)")]
        public bool undeploy = false;

        [clp.FlagUsage("Unpublish the policy (default is to publish, implies undeploy)")]
        public bool unpublish = false;

        [clp.ValueUsage("Rule set version to operate on.", ValueName = "ruleSetVersion", Optional = true)]
        public string ruleSetVersion = "";

        public void PrintLogo()
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(base.GetLogo(false));
            Console.ForegroundColor = originalColor;
        }
    }
}
