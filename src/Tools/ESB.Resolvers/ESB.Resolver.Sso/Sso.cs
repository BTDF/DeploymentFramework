// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace DeploymentFrameworkForBizTalk.ESB.Resolver.Sso
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "http://schemas.microsoft.biztalk.practices.esb.com/itinerary", IsNullable = false)]
    [DesignerCategory("code")]
    public class Sso
    {
        // Properties
        [XmlAttribute]
        public string Action { get; set; }
        [XmlAttribute]
        public string EndpointConfig { get; set; }
        [XmlAttribute]
        public string JaxRpcResponse { get; set; }
        [XmlAttribute]
        public string MessageExchangePattern { get; set; }
        [XmlAttribute]
        public string TargetNamespace { get; set; }
        [XmlAttribute]
        public string TransformType { get; set; }
        [XmlAttribute]
        public string TransportLocation { get; set; }
        [XmlAttribute]
        public string TransportType { get; set; }
        [XmlAttribute]
        public string AffiliateAppName { get; set; }
    }
}
