// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.IO;
using System.Configuration;
using System.Net;
using System.Web;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using DeploymentFramework.IIS.Test.ProcessPO;

namespace DeploymentFramework.IIS.Test
{
    [TestFixture]
    public class VerifyDeployment
    {
        [Test]
        public void ProcessPO()
        {
            ProcessPOResponse response = null;
            ProcessPORequest request = new ProcessPORequest();
            request.PO = new POType();
            request.PO.PO_Number = "1234";
            request.PO.Total = "$19.99";

            ProcessPOServiceClient client = null;

            try
            {
                client = new ProcessPOServiceClient();
                client.Open();
                response = client.ProcessPO(request);
                client.Close();
            }
            catch
            {
                if (client != null)
                {
                    client.Abort();
                }
                throw;
            }

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Invoice);
            Assert.AreEqual(response.Invoice.TotalPrice, "$19.99");
        }
    }
}
