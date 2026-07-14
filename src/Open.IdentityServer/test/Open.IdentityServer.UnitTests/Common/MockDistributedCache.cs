// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common;

internal class MockDistributedCache : IDistributedCache
{
    public readonly ConcurrentDictionary<string, byte[]> Entries = new();

    public byte[] Get(string key)
    {
        return Entries.TryGetValue(key, out var value)
            ? (byte[])value.Clone()
            : null;
    }

    public Task<byte[]> GetAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(Get(key));
    }

    public void Refresh(string key)
    {
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        Entries.TryRemove(key, out _);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        Remove(key);
        return Task.CompletedTask;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        Entries[key] = (byte[])value.Clone();
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        Set(key, value, options);
        return Task.CompletedTask;
    }
}
