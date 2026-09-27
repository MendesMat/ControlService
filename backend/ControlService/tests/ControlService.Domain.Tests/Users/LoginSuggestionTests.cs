using ControlService.Domain.Users;

namespace ControlService.Domain.Tests.Users;

public class LoginSuggestionTests
{
    [Fact]
    public void Full_name_gives_first_and_last_name_with_a_dot() // USR-14
    {
        var suggestion = LoginSuggestion.Suggest("Ana Paula Souza", takenLogins: []);

        suggestion.ShouldBe("ana.souza");
    }
}
