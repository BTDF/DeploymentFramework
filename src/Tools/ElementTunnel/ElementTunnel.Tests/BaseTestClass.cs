// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.
//
// Contributions to ElementTunnel by Tim Rayburn
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.IO;

namespace ElementTunnel.Tests
{
    [TestClass]
    public class BaseTestClass
    {
        protected ElementTunneler tunneler;
        protected XmlDocument inDoc;
        protected List<string> xPaths;

        protected string clearData;
        protected string encodedData;

        [TestInitialize]
        public void Setup()
        {
            tunneler = new ElementTunneler();
            inDoc = new XmlDocument();
            xPaths = new List<string>();
            clearData = TestExtensions.encodeTarget;
            encodedData = TestExtensions.encodedTarget;
        }

        [TestCleanup]
        public void Cleanup()
        {
        }
    }
}
