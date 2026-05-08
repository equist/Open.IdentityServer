using System;

namespace Open.IdentityServer.Models;

/// <summary>
/// Defines token expiration mode (Unused, added for compatibility)
/// </summary>
[Flags]
public enum DPoPTokenExpirationValidationMode
{
    /// <summary>
    /// Custom value (Unused, added for compatibility) 
    /// </summary>
    Custom = 0,
    
    /// <summary>
    /// Iat value (Unused, added for compatibility)
    /// </summary>
    Iat = 1,
    
    /// <summary>
    /// Nonce value (Unused, added for compatibility) 
    /// </summary>
    Nonce = 2,
    
    /// <summary>
    /// IatAndNonce value (Unused, added for compatibility) 
    /// </summary>
    IatAndNonce = Iat | Nonce
}