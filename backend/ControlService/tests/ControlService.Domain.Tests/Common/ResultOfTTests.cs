using ControlService.Domain.Common;

namespace ControlService.Domain.Tests.Common;

public class ResultOfTTests
{
    [Fact]
    public void Result_of_T_success_exposes_value() // ADR-0009
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Result_of_T_failure_does_not_expose_a_value() // ADR-0009
    {
        var error = new Error("not_found", "Registro não encontrado.");

        var result = Result<int>.Failure(error);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }
}
