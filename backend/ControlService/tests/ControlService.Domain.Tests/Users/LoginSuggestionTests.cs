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

    [Fact]
    public void Suggestion_has_no_accents_and_is_lowercase() // USR-14
    {
        var suggestion = LoginSuggestion.Suggest("Ângela Brandão", takenLogins: []);

        suggestion.ShouldBe("angela.brandao");
    }

    [Fact]
    public void One_word_name_gives_that_word() // USR-15
    {
        var suggestion = LoginSuggestion.Suggest("Madalena", takenLogins: []);

        suggestion.ShouldBe("madalena");
    }
}
