// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.EntityFramework.Stores;

/// <summary>
/// Duende key material store
/// </summary>
public class IdentityServerKeyStore(IIdentityServerCompatibilityDbContext dbContext): IIdentityServerKeyStore
{
    /// <summary>
    /// Gets all keys stored in Duende key store
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IdentityServerKeyMaterial> GetKeys() => dbContext.Keys;
}