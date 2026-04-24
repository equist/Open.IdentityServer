#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.Extensions;

/// <summary>
/// Extensions for <see cref="ResourceValidationResult"/>
/// </summary>
public static class ResourceValidationResultExtensions
{
    extension(ResourceValidationResult resourceValidationResult)
    {
        /// <summary>
        /// Function to apply downscoping to <see cref="ResourceValidationResult"/> using a <see cref="ValidatedTokenRequest"/>
        /// if resource indicator is provided.
        /// </summary>
        /// <param name="tokenRequest"></param>
        public void DownscopeWhenResourceIndicators(ValidatedTokenRequest tokenRequest)
        {
            if (!tokenRequest.RequestedResourceIndicator.IsPresent())
            {
                return;
            }
            
            List<string> allowedResourceIndicators = [];

            if (tokenRequest.AuthorizationCode != null)
            {
                allowedResourceIndicators = tokenRequest.AuthorizationCode.RequestedResourceIndicators?.ToList() ?? [];
            }

            if (tokenRequest.RefreshToken != null)
            {
                allowedResourceIndicators = tokenRequest.RefreshToken.AuthorizedResourceIndicators?.ToList() ?? [];
            }
            
            if (!allowedResourceIndicators.Any())
            {
                allowedResourceIndicators.Add(tokenRequest.RequestedResourceIndicator);
            }
            else
            {
                var foundResourceIndicator = allowedResourceIndicators
                    .First(x => x == tokenRequest.RequestedResourceIndicator);

                allowedResourceIndicators = [foundResourceIndicator];
            }
                
            resourceValidationResult.FilterUsingResourceIndicators(allowedResourceIndicators);
        }
    }
}