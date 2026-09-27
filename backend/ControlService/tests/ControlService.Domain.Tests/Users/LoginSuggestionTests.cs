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

    [Theory]
    [InlineData("Joana D'Ávila", "joana.davila")]
    [InlineData("  Ana   Souza  ", "ana.souza")]
    public void Characters_outside_the_login_rule_are_removed(string fullName, string expected) // USR-15
    {
        var suggestion = LoginSuggestion.Suggest(fullName, takenLogins: []);

        suggestion.ShouldBe(expected);
    }

    [Fact]
    public void Taken_login_gets_number_2() // USR-14
    {
        var suggestion = LoginSuggestion.Suggest("Ana Souza", takenLogins: ["ana.souza"]);

        suggestion.ShouldBe("ana.souza2");
    }

    [Fact]
    public void Number_grows_until_the_login_is_free() // USR-14
    {
        var suggestion = LoginSuggestion.Suggest("Ana Souza", takenLogins: ["ana.souza", "ana.souza2"]);

        suggestion.ShouldBe("ana.souza3");
    }
}
