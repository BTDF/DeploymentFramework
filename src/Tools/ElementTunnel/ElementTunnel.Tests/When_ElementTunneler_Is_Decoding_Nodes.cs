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
    public class When_ElementTunneler_Is_Decoding_Nodes : BaseTestClass
    {
        [TestMethod]
        public void It_Should_Decode_One_Level_Properly()
        {
            var inData = "<a><b>&lt;c /&gt;&lt;c /&gt;&lt;c /&gt;</b></a>";
            inDoc.LoadXml(inData);
            xPaths.Add("/a/b");

            tunneler.TunnelXPaths(inDoc, xPaths, false);

            inDoc.DocumentElement.FirstChild.ChildNodes.Count.ShouldBe(3);
        }

        [TestMethod]
        public void It_Should_Decode_Multiple_Levels_Properly()
        {
            var inData = "<a><b>&lt;c&gt;&amp;lt;d&amp;gt;123456&amp;lt;/d&amp;gt;&lt;/c&gt;</b></a>";
            inDoc.LoadXml(inData);
            xPaths.Add("/a/b/c");
            xPaths.Add("/a/b");

            tunneler.TunnelXPaths(inDoc, xPaths, false);

            inDoc.DocumentElement.FirstChild.ChildNodes.Count.ShouldBe(1);
            inDoc.DocumentElement.FirstChild.FirstChild.InnerText.ShouldBe("123456");
        }

        [TestMethod]
        public void It_Should_Decode_Only_Xml_Special_Chars()
        {
            var inData = "<a><b>&lt;c&gt;&amp;lt;d&amp;gt;&apos;&quot;123456&quot;&apos;&amp;lt;/d&amp;gt;&lt;/c&gt;</b></a>";
            inDoc.LoadXml(inData);
            xPaths.Add("/a/b/c");
            xPaths.Add("/a/b");

            tunneler.TunnelXPaths(inDoc, xPaths, false);

            inDoc.DocumentElement.FirstChild.ChildNodes.Count.ShouldBe(1);
            inDoc.DocumentElement.FirstChild.FirstChild.InnerText.ShouldBe("'\"123456\"'");
        }
    }
}
