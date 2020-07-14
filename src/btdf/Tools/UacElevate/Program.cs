// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DeploymentFramework.UacElevate
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: UacElevate.exe <executable> <arguments>");
                return;
            }

            Process.Start(args[0], args.Length > 1 ? args[1] : null);
        }
    }
}
