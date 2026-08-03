// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Xunit;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_ClientValidation_Invalid
{
    private const string Category = "AuthorizeRequest Client Validation - Invalid";

    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_Protocol_Client()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "wsfed" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://wsfed/callback" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdToken }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
    }
}