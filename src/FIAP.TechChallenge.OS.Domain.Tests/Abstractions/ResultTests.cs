using System.Text.Json;
using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Domain.Tests;

public class ResultTests
{
    [TestCase("""{"IsSuccess":true,"Value":"ok","Error":null}""")]
    [TestCase("""{"IsSuccess":false,"Value":null,"Error":null}""")]
    [TestCase("""{"IsSuccess":true,"Value":"ok"}""")]
    public void Deserialize_ShouldRejectMissingError_BeforeEvaluatingResult(string json)
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            JsonSerializer.Deserialize<Result<string>>(json));

        Assert.That(exception!.ParamName, Is.EqualTo("error"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Deserialize_ShouldPreserveValidResult_WhenRoundTripping(bool success)
    {
        var original = success
            ? Result<string>.Success("ok")
            : Result<string>.Failure(Error.NotFound("Ordem"));

        var restored = JsonSerializer.Deserialize<Result<string>>(JsonSerializer.Serialize(original))!;

        Assert.Multiple(() =>
        {
            Assert.That(restored.IsSuccess, Is.EqualTo(original.IsSuccess));
            Assert.That(restored.Value, Is.EqualTo(original.Value));
            Assert.That(restored.Error, Is.EqualTo(original.Error));
        });
    }
}
