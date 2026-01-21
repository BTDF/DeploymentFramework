// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
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
