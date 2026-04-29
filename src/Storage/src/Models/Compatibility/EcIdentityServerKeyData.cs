// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Cryptography;

namespace Open.IdentityServer.Models;

/// <summary>
/// EcKey class representing EC key data field in database
/// </summary>
public class EcIdentityServerKeyData: IdentityServerKeyData
{
    /// <summary>
    /// Get or set D value
    /// </summary>
    public byte[] D { get; set; }
    
    /// <summary>
    /// Get or set Q value
    /// </summary>
    public ECPoint Q { get; set; }
}