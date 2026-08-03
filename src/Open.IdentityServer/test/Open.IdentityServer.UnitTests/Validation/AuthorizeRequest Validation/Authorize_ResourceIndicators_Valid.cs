// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_ResourceIndicators_Valid
{
    private const string Category = "AuthorizeRequest Resource Indicators - Valid";
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_ResourceIndicators_WithCodeFlow()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid urn:valid.resource:Read valid:All" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
            { OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query },
            { OidcConstants.AuthorizeRequest.Resource, "urn:valid.resource" },
            { OidcConstants.AuthorizeRequest.Resource, "https://valid.resource.com" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeFalse();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_ResourceIndicators_WithHybridFlow()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "hybridclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid urn:valid.resource:Read valid:All" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.Nonce, "nonce" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.CodeIdToken },
            { OidcConstants.AuthorizeRequest.Resource, "urn:valid.resource https://valid.resource.com" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeFalse();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_ResourceIndicators_WithImplicitFlow()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, "urn:valid.resource:Read valid:All" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token },
            { OidcConstants.AuthorizeRequest.Resource, "urn:valid.resource" },
            { OidcConstants.AuthorizeRequest.Resource, "https://valid.resource.com" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeFalse();
    }
}