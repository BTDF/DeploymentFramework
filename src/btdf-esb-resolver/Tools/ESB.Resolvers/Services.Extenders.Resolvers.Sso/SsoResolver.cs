// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.Modeling.ExtensionProvider.Extension;
using Microsoft.Practices.Modeling.ExtensionProvider.Metadata;
using Microsoft.Practices.Services.ItineraryDsl;

namespace DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso
{
    [Serializable]
    [ObjectExtender(typeof(Resolver))]
    public class SsoResolver : ObjectExtender<Resolver>
    {
        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Name of the SSO affiliate application, usually the value of ProjectName in the .BTDFPROJ file.")]
        [DisplayName("AffiliateAppName")]
        [ReadOnly(false)]
        [Browsable(true)]
        [StringLengthValidator(1, RangeBoundaryType.Inclusive, 0, RangeBoundaryType.Ignore, MessageTemplate = "The 'AffiliateAppName' property should not be null or empty.")]
        public string AffiliateAppName { get; set; }

        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Name of a row in the SettingsFileGenerator spreadsheet that specifies the action.")]
        [DisplayName("Action")]
        [ReadOnly(false)]
        [Browsable(true)]
        public string Action { get; set; }

        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Name of a row in the SettingsFileGenerator spreadsheet that specifies the endpoint configuration.")]
        [DisplayName("Endpoint Configuration")]
        [ReadOnly(false)]
        [Browsable(true)]
        public string EndpointConfig { get; set; }

        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Specifies the JAX RPC response.")]
        [DisplayName("Jax Rpc Response")]
        [ReadOnly(false)]
        [Browsable(true)]
        public bool JaxRpcResponse { get; set; }

        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Name of a row in the SettingsFileGenerator spreadsheet that specifies the message exchange pattern.")]
        [DisplayName("Message Exchange Pattern")]
        [ReadOnly(false)]
        [Browsable(true)]
        public string MessageExchangePattern { get; set; }

        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Name of a row in the SettingsFileGenerator spreadsheet that specifies the target namespace.")]
        [DisplayName("Target Namespace")]
        [ReadOnly(false)]
        [Browsable(true)]
        public string TargetNamespace { get; set; }

        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Name of a row in the SettingsFileGenerator spreadsheet that specifies the transform type.")]
        [DisplayName("Transform Type")]
        [ReadOnly(false)]
        [Browsable(true)]
        public string TransformType { get; set; }

        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Name of a row in the SettingsFileGenerator spreadsheet that specifies the transport location.")]
        [DisplayName("Transport Location")]
        [ReadOnly(false)]
        [Browsable(true)]
        public string TransportLocation { get; set; }

        [Category(SsoResolverExtensionProvider.ExtensionProviderPropertyCategory)]
        [Description("Name of a row in the SettingsFileGenerator spreadsheet that specifies the transport name.")]
        [DisplayName("Transport Name")]
        [ReadOnly(false)]
        [Browsable(true)]
        public string TransportType { get; set; }
    }
}
