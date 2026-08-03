// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.UnitTests.Endpoints.Authorize;

public class StubAuthorizeRequestValidator : IAuthorizeRequestValidator
{
    public AuthorizeRequestValidationResult Result { get; set; }

    public Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null)
    {
        return ValidateAsync(new AuthorizeRequestValidationContext(parameters, subject));
    }

    public Task<AuthorizeRequestValidationResult> ValidateAsync(AuthorizeRequestValidationContext context)
    {
        return Task.FromResult(Result);
    }
}