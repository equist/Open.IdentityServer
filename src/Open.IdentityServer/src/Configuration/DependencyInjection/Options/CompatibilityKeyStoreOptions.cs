using System;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// Compatibility key store options.
/// </summary>
public class CompatibilityKeyStoreOptions
{
    /// <summary>
    /// Maximum lifetime for keys read from the key store, defaults to 90 days.
    /// </summary>
    public TimeSpan MaxLifetime { get; set; } = TimeSpan.FromDays(90);
}