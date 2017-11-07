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
    public class When_ElementTunneler_Is_Decoding_Nodes : BaseTestClass
    {
        [TestMethod]
        public void It_Should_Decode_One_Level_Properly()
        {
            string inData = @"<a><b>&lt;c /&gt;&lt;c /&gt;&lt;c /&gt;</b></a>";
            inDoc.LoadXml(inData);
            xPaths.Add("/a/b");

            tunneler.TunnelXPaths(inDoc, xPaths, false, true);

            inDoc.DocumentElement.FirstChild.ChildNodes.Count.ShouldBe(3);
        }

        [TestMethod]
        public void It_Should_Decode_Multiple_Levels_Properly()
        {
            string inData = @"<a><b>&lt;c&gt;&amp;lt;d&amp;gt;123456&amp;lt;/d&amp;gt;&lt;/c&gt;</b></a>";
            inDoc.LoadXml(inData);
            xPaths.Add("/a/b/c");
            xPaths.Add("/a/b");

            tunneler.TunnelXPaths(inDoc, xPaths, false, true);

            inDoc.DocumentElement.FirstChild.ChildNodes.Count.ShouldBe(1);
            inDoc.DocumentElement.FirstChild.FirstChild.InnerText.ShouldBe("123456");
        }

        [TestMethod]
        public void It_Should_Decode_Only_Xml_Special_Chars()
        {
            string inData = @"<a><b>&lt;c&gt;&amp;lt;d&amp;gt;'""123456""'&amp;lt;/d&amp;gt;&lt;/c&gt;</b></a>";
            inDoc.LoadXml(inData);
            xPaths.Add("/a/b/c");
            xPaths.Add("/a/b");

            tunneler.TunnelXPaths(inDoc, xPaths, false, true);

            inDoc.DocumentElement.FirstChild.ChildNodes.Count.ShouldBe(1);
            inDoc.DocumentElement.FirstChild.FirstChild.InnerText.ShouldBe("'\"123456\"'");
        }

        [TestMethod]
        public void It_Should_Decode_Only_Encoded_Xml()
        {
            string inData = @"<a><b><c>&lt;d&gt;'""123456""'&lt;/d&gt;</c></b></a>";
            inDoc.LoadXml(inData);
            xPaths.Add("/a/b/c");
            xPaths.Add("/a/b");

            tunneler.TunnelXPaths(inDoc, xPaths, false, true);

            var outData = @"<a><b><c><d>'""123456""'</d></c></b></a>";

            Assert.IsTrue(string.Compare(inDoc.OuterXml, outData, true) == 0);
        }

        [TestMethod]
        public void It_Should_Remove_Processing_Instruction()
        {
            string inData =
                @"<Filter>&lt;?xml version=""1.0"" encoding=""utf-16""?&gt;"
                + @"&lt;Filter xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;"
                + @" &lt;Group&gt;"
                + @"    &lt;Statement Property=""Microsoft.Practices.ESB.Itinerary.Schemas.ServiceName"" Operator=""0"" Value=""TestApp"" /&gt;"
                + @"    &lt;Statement Property=""Microsoft.Practices.ESB.Itinerary.Schemas.ServiceState"" Operator=""0"" Value=""Pending"" /&gt;"
                + @"    &lt;Statement Property=""Microsoft.Practices.ESB.Itinerary.Schemas.ServiceType"" Operator=""0"" Value=""Messaging"" /&gt;"
                + @"  &lt;/Group&gt;"
                + @"&lt;/Filter&gt;</Filter>";

            inDoc.LoadXml(inData);
            xPaths.Add("/Filter");

            tunneler.TunnelXPaths(inDoc, xPaths, false, true);

            string outData =
                @"<Filter>"
                + @"<Filter xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">"
                + @" <Group>"
                + @"    <Statement Property=""Microsoft.Practices.ESB.Itinerary.Schemas.ServiceName"" Operator=""0"" Value=""TestApp"" />"
                + @"    <Statement Property=""Microsoft.Practices.ESB.Itinerary.Schemas.ServiceState"" Operator=""0"" Value=""Pending"" />"
                + @"    <Statement Property=""Microsoft.Practices.ESB.Itinerary.Schemas.ServiceType"" Operator=""0"" Value=""Messaging"" />"
                + @"  </Group>"
                + @"</Filter></Filter>";

            Assert.IsTrue(string.Compare(inDoc.OuterXml, outData, true) == 0);
        }

        [TestMethod]
        public void It_Should_Not_Destroy_Meaningful_Whitespace()
        {
            string inData =
                @"<Filter>"
                + @" &lt;Statement Property=""Separator"" Value="" "" /&gt;"
                + @"</Filter>";

            inDoc.LoadXml(inData);
            xPaths.Add("/Filter");

            tunneler.TunnelXPaths(inDoc, xPaths, false, true);

            string outData =
                @"<Filter>"
                + @" <Statement Property=""Separator"" Value="" "" />"
                + @"</Filter>";

            Assert.IsTrue(string.Compare(inDoc.OuterXml, outData, true) == 0);
        }
    }
}
