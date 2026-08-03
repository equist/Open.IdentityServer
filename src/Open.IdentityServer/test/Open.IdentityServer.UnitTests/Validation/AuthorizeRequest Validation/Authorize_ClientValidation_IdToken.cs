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

public class Authorize_ClientValidation_IdToken
{
    [Fact]
    [Trait("Category", "AuthorizeRequest Client Validation - IdToken")]
    public async Task Mixed_IdToken_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid resource" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdToken },
            { OidcConstants.AuthorizeRequest.Nonce, "abc" }
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(new AuthorizeRequestValidationContext(parameters));
            
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidScope);
    }
}