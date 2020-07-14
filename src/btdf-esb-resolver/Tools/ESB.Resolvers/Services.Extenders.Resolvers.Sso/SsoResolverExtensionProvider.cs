// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Modeling.ExtensionProvider.Extension;
using Microsoft.Practices.Modeling.ExtensionProvider.Metadata;
using Microsoft.Practices.Services.ItineraryDsl;

namespace DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso
{
    [ResolverExtensionProvider]
    [ExtensionProvider("D5553443-7CF2-4b36-8ED0-E7873F73FB3F", "BTDF-SSO", "Deployment Framework for BizTalk SSO Resolver Extension", typeof(ItineraryDslDomainModel))]
    public class SsoResolverExtensionProvider : ExtensionProviderBase
    {
        public SsoResolverExtensionProvider()
            : base(new Type[] { typeof(SsoResolver) })
        {
        }
    }
}
