// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Retrieval of key material
/// </summary>
public interface IIdentityServerKeyStore
{
    /// <summary>
    /// Get all key material stored in databased
    /// </summary>
    /// <returns></returns>
    IEnumerable<IdentityServerKeyMaterial> GetKeys();
}