using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Xunit;

namespace IdentityServer.UnitTests.Extensions;

public class AuthorizeResponseExtensionsTests
{
    [Fact]
    public void ToNameValueCollection_WhenErrorAndIssuerSet_ShouldHaveIssPropertyInCollection()
    {
        var response = new AuthorizeResponse
        {
            Error = "Fake Error",
            Issuer = "https://issuer.com",
        };

        var collection = response.ToNameValueCollection();

        collection["iss"].Should().BeEquivalentTo(response.Issuer);
    }
    
    [Fact]
    public void ToNameValueCollection_WhenNonErrorAndIssuerSet_ShouldHaveIssPropertyInCollection()
    {
        var response = new AuthorizeResponse
        {
            Code = "FakeCodeResponse",
            Issuer = "https://issuer.com",
        };

        var collection = response.ToNameValueCollection();

        collection["iss"].Should().BeEquivalentTo(response.Issuer);
    }
}