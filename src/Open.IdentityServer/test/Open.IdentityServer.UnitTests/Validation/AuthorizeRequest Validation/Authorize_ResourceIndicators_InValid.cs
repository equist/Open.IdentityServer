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

public class Authorize_ResourceIndicators_InValid
{
    private const string Category = "AuthorizeRequest Resource Indicators - Invalid";

    [Fact]
    [Trait("Category", Category)]
    public async Task InValid_ResourceIndicators_AreMalformedNonUrl()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
            { OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query },
            { OidcConstants.AuthorizeRequest.Resource, "invalid_resource" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task InValid_ResourceIndicators_AreNotRegistered()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
            { OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query },
            { OidcConstants.AuthorizeRequest.Resource, "https://other.resource.com" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task InValid_ResourceIndicators_NoScopesFromResourcesRequested()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid valid:Read" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
            { OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query },
            { OidcConstants.AuthorizeRequest.Resource, "urn:valid.resource" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task InValid_ResourceIndicators_ContainsQuery()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid valid:Read" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
            { OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query },
            { OidcConstants.AuthorizeRequest.Resource, "https://valid.resource.com?some=val" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task InValid_ResourceIndicators_ContainsFragment()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid valid:Read" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
            { OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query },
            { OidcConstants.AuthorizeRequest.Resource, "https://valid.resource.com#some=val" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }

    [Fact(Skip = "Skipping to disable as this case already handled by scope validation")]
    [Trait("Category", Category)]
    public async Task Invalid_ResourceIndicators_UnauthorisedResource()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid unauth:Read" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
            { OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query },
            { OidcConstants.AuthorizeRequest.Resource, "urn:unauth.resource" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidTarget);
    }
}