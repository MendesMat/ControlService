using ControlService.Domain.Common;

namespace ControlService.Domain.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void Error_stores_code_and_message() // ADR-0009
    {
        var error = new Error("validation_failed", "Alguns campos precisam ser corrigidos.");

        error.Code.ShouldBe("validation_failed");
        error.Message.ShouldBe("Alguns campos precisam ser corrigidos.");
    }
}
