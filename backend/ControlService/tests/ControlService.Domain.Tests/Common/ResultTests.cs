using ControlService.Domain.Common;

namespace ControlService.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Result_success_has_no_error() // ADR-0009
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Result_failure_exposes_error() // ADR-0009
    {
        var error = new Error("validation_failed", "Alguns campos precisam ser corrigidos.");

        var result = Result.Failure(error);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }
}
