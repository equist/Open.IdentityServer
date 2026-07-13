// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using IdentityServer.UnitTests.Common;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores.Default;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.UnitTests.Stores.Default;

public class DistributedCacheAuthorizationParametersMessageStoreTests
{
    private MockDistributedCache _mockCache = new();

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenUnspecifiedDateTime_ShouldTreatAsUtcTime()
    {
        var subject = new DistributedCacheAuthorizationParametersMessageStore(_mockCache, new DefaultHandleGenerationService());

        _mockCache.Entries.Clear();

        var id = await subject.WriteAsync(new Message<IDictionary<string, string[]>>(new Dictionary<string, string[]>()));
        _mockCache.Entries.Count.Should().Be(1);

        await subject.DeleteAsync(id);
        _mockCache.Entries.Count.Should().Be(0);
    }
}
