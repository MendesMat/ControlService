namespace ControlService.Application.Users.SuggestLogin;

/// <summary>`Login` is empty when there is nothing to suggest (USR-15).</summary>
public sealed record SuggestLoginResponse(string Login);
