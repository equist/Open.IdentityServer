using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Stores.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IdentityServer.UnitTests.DataProtection;

public class FakeData
{
    public string Value1 { get; set; }
}

public class PersistentGrantSerializerDataProtectionDecoratorTests
{
    private readonly IPersistentGrantSerializer decoratedSerializer = Mock.Of<IPersistentGrantSerializer>();
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly IDataProtector dataProtector = Mock.Of<IDataProtector>();

    public PersistentGrantSerializerDataProtectionDecoratorTests()
    {
        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector(It.IsAny<string>()))
            .Returns(dataProtector);
    }

    private PersistentGrantSerializerDataProtectionDecorator CreateSut()
    {
        return new PersistentGrantSerializerDataProtectionDecorator(decoratedSerializer, dataProtectionProvider);
    }

    [Fact]
    public void Serialize_WhenProtectionEnabled_ShouldProtectDataAndWrapInDataProtectedDataObject()
    {
        var input = new FakeData { Value1 = "someVal" };
        var inputSerialised = "{ \"value1\" = \"someVal\" }";

        var protectedBytes = Encoding.UTF8.GetBytes("PROTECTED_INPUT");
        var expectedPayload = Convert.ToBase64String(protectedBytes);

        Mock.Get(decoratedSerializer)
            .Setup(x => x.Serialize(input))
            .Returns(inputSerialised);

        Mock.Get(dataProtector)
            .Setup(x => x.Protect(It.Is<byte[]>(
                b => Encoding.UTF8.GetString(b) == inputSerialised)))
            .Returns(protectedBytes);

        var sut = CreateSut();
        var actual = sut.Serialize(input);

        actual.Should().NotBeNullOrWhiteSpace();
        JsonElement element = JsonElement.Parse(actual);

        element.GetProperty("PersistentGrantDataContainerVersion").GetInt32().Should().Be(1);
        element.GetProperty("DataProtected").GetBoolean().Should().BeTrue();
        element.GetProperty("Payload").GetString().Should().BeEquivalentTo(expectedPayload);
    }

    [Fact]
    public void Deserialize_WhenProtectedDataDeserialisationFails_ShouldThrowException()
    {
        var input = "null";

        var sut = CreateSut();
        Action act = () => sut.Deserialize<FakeData>(input);

        act.Should().ThrowExactly<Exception>();
    }

    [Fact]
    public void Deserialize_WhenUnprotectThrowsException_ShouldThrowDataProtectionException()
    {
        CryptographicException fakeException = new CryptographicException();
        FakeData payload = new() { Value1 = "someVal" };
        var serialisedPayload = "{ \"value1\" = \"someVal\" }";

        var protectedPayloadBytes = Encoding.UTF8.GetBytes("PROTECTED_INPUT");
        var protectedPayloadBase64 = Convert.ToBase64String(protectedPayloadBytes);

        var input = """
                    {
                        "PersistentGrantDataContainerVersion": 1,
                        "DataProtected": true,
                        "Payload": "{ \u0022value1\u0022 = \u0022someVal\u0022 }"
                    }
                    """;

        Mock.Get(decoratedSerializer)
            .Setup(x => x.Deserialize<FakeData>(serialisedPayload))
            .Returns(payload);

        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == protectedPayloadBase64)))
            .Throws<CryptographicException>();

        var sut = CreateSut();
        Action act = () => sut.Deserialize<FakeData>(input);

        act.Should().ThrowExactly<DataProtectionException>()
            .WithInnerException(fakeException.GetType());
    }

    [Fact]
    public void Deserialize_WhenProtectedDataProvided_ShouldUnprotectAndReturn()
    {
        FakeData payload = new() { Value1 = "someVal" };
        var serialisedPayload = "{ \"value1\" = \"someVal\" }";

        var protectedPayloadBytes = Encoding.UTF8.GetBytes("PROTECTED_INPUT");
        var protectedPayloadBase64 = Convert.ToBase64String(protectedPayloadBytes);

        var input = $$"""
                      {
                          "PersistentGrantDataContainerVersion": 1,
                          "DataProtected": true,
                          "Payload": "{{protectedPayloadBase64}}"
                      }
                      """;

        Mock.Get(decoratedSerializer)
            .Setup(x => x.Deserialize<FakeData>(serialisedPayload))
            .Returns(payload);

        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == protectedPayloadBase64)))
            .Returns(Encoding.UTF8.GetBytes(serialisedPayload));

        var sut = CreateSut();
        var actual = sut.Deserialize<FakeData>(input);

        actual.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public void Deserialize_WhenUnProtectedDataProvided_ShouldReturnData()
    {
        FakeData payload = new() { Value1 = "someVal" };
        var serialisedPayload = "{ \"value1\" = \"someVal\" }";

        var input = """
                    {
                        "PersistentGrantDataContainerVersion": 1,
                        "DataProtected": false,
                        "Payload": "{ \u0022value1\u0022 = \u0022someVal\u0022 }"
                    }
                    """;

        Mock.Get(decoratedSerializer)
            .Setup(x => x.Deserialize<FakeData>(serialisedPayload))
            .Returns(payload);

        var sut = CreateSut();
        var actual = sut.Deserialize<FakeData>(input);

        actual.Should().BeEquivalentTo(payload);
    }
}