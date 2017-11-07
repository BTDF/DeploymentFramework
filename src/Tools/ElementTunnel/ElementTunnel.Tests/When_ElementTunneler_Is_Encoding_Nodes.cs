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
    public class When_ElementTunneler_Is_Encoding_Nodes : BaseTestClass
    {
        [TestMethod]
        public void It_Should_Handle_One_Level_Of_Encoding()
        {
            var inputXml = string.Format("<foo><bar>{0}</bar></foo>", clearData);
            string targetXPath = "/foo/bar";

            inDoc.LoadXml(inputXml);
            xPaths.Add(targetXPath);

            tunneler.TunnelXPaths(inDoc, xPaths, true, true);

            inDoc.ForEach(targetXPath, i => i.ShouldBeEncoded());
        }

        [TestMethod]
        public void It_Should_Not_Add_Text_To_Valueless_Target_Nodes()
        {
            var inputXml = string.Format("<foo><bar></bar></foo>", clearData);
            string targetXPath = "/foo/bar";

            inDoc.LoadXml(inputXml);
            xPaths.Add(targetXPath);

            tunneler.TunnelXPaths(inDoc, xPaths, true, true);

            inDoc.ForEach(targetXPath, i => i.InnerText.Length.Equals(0));
            inDoc.ForEach(targetXPath, i => i.OuterXml.ShouldBe("<bar></bar>"));
        }

        [TestMethod]
        public void It_Should_Not_Add_Text_To_Empty_Target_Nodes()
        {
            var inputXml = string.Format("<foo><bar /></foo>", clearData);
            string targetXPath = "/foo/bar";

            inDoc.LoadXml(inputXml);
            xPaths.Add(targetXPath);

            tunneler.TunnelXPaths(inDoc, xPaths, true, true);

            inDoc.ForEach(targetXPath, i => i.InnerText.Length.Equals(0));
            inDoc.ForEach(targetXPath, i => i.OuterXml.ShouldBe("<bar />"));
        }

        [TestMethod]
        public void It_Should_Handle_Multiple_Nodes()
        {
            var inputXml = string.Format("<foo><bar>{0}</bar><bar>{0}</bar><bar>{0}</bar><bar>{0}</bar></foo>", clearData);
            string targetXPath = "/foo/bar";

            inDoc.LoadXml(inputXml);
            xPaths.Add(targetXPath);

            tunneler.TunnelXPaths(inDoc, xPaths, true, true);

            inDoc.ForEach(targetXPath, i => i.ShouldBeEncoded());
        }

        [TestMethod]
        public void It_Should_Handle_Multiple_Levels_Of_Encoding()
        {
            var inputXml = "<a><b><c><d /></c></b></a>";
            var expectedResult = "&lt;c&gt;&amp;lt;d /&amp;gt;&lt;/c&gt;";
            string targetXPath = "/a/b";

            inDoc.LoadXml(inputXml);
            xPaths.Add("/a/b/c");
            xPaths.Add(targetXPath);

            tunneler.TunnelXPaths(inDoc, xPaths, true, true);

            inDoc.ForEach(targetXPath, i => i.InnerXml.ShouldBe(expectedResult));
        }

        //[TestMethod]
        //public void It_Should_Keep_Whitespace()
        //{
        //    var inputXml = "<a><b><c>  <d />  </c></b></a>";
        //    var expectedResult = "&lt;c&gt;  &amp;lt;d /&amp;gt;  &lt;/c&gt;";
        //    string targetXPath = "/a/b";

        //    inDoc.LoadXml(inputXml);
        //    xPaths.Add("/a/b/c");
        //    xPaths.Add(targetXPath);

        //    tunneler.TunnelXPaths(inDoc, xPaths, true, true);

        //    inDoc.ForEach(targetXPath, i => i.InnerXml.ShouldBe(expectedResult));
        //}
    }
}
