// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_ProtocolValidation_CustomValidator
{
    private const string Category = "AuthorizeRequest Protocol Validation";

    private StubAuthorizeRequestValidator _stubAuthorizeRequestValidator = new();
    private AuthorizeRequestValidator _subject;

    public Authorize_ProtocolValidation_CustomValidator()
    {
        _subject = Factory.CreateAuthorizeRequestValidator(customValidator: _stubAuthorizeRequestValidator);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task should_call_custom_validator()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code }
        };

        var result = await _subject.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        _stubAuthorizeRequestValidator.WasCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task should_return_error_info_from_custom_validator()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code }
        };

        _stubAuthorizeRequestValidator.Callback = ctx =>
        {
            ctx.Result = new AuthorizeRequestValidationResult(ctx.Result.ValidatedRequest, "foo", "bar");
        };
        var result = await _subject.ValidateAsync(new AuthorizeRequestValidationContext(parameters));

        result.IsError.Should().BeTrue();
        result.Error.Should().Be("foo");
        result.ErrorDescription.Should().Be("bar");
    }
}

public class StubAuthorizeRequestValidator : ICustomAuthorizeRequestValidator
{
    public Action<CustomAuthorizeRequestValidationContext> Callback;
    public bool WasCalled { get; set; }

    public Task ValidateAsync(CustomAuthorizeRequestValidationContext context)
    {
        WasCalled = true;
        Callback?.Invoke(context);
        return Task.CompletedTask;
    }
}