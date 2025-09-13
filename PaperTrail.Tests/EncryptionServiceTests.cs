using FluentAssertions;
using PaperTrail.Core.Services;
using Xunit;

namespace PaperTrail.Tests;

public class EncryptionServiceTests
{
    [Fact]
    public void RoundTrip()
    {
        var service = new EncryptionService();
        var cipher = service.Encrypt("hello", "password");
        var plain = service.Decrypt(cipher, "password");
        plain.Should().Be("hello");
    }
}
