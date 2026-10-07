using ControlService.Domain.Common;

namespace ControlService.Domain.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void Error_stores_code_and_message()
    {
        var error = new Error("validation_failed", "Alguns campos precisam ser corrigidos.");

        error.Code.ShouldBe("validation_failed");
        error.Message.ShouldBe("Alguns campos precisam ser corrigidos.");
    }

    [Fact]
    public void Error_can_be_created_with_field_errors()
    {
        var fields = new Dictionary<string, string[]>
        {
            ["emergencyContact.phone"] = ["Digite o telefone com DDD. Ex.: (21) 98765-4321."]
        };

        var error = new Error("validation_failed", "Alguns campos precisam ser corrigidos.", Fields: fields);

        error.Fields.ShouldBe(fields);
    }

    [Fact]
    public void Error_can_be_created_with_details()
    {
        var details = new Dictionary<string, object?> { ["updatedByName"] = "Bruno Lima" };

        var error = new Error("concurrency_conflict", "Este cadastro foi alterado.", Details: details);

        error.Details.ShouldBe(details);
    }
}
