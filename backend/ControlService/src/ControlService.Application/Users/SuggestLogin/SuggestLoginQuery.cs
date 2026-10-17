namespace ControlService.Application.Users.SuggestLogin;

/// <summary>The full name is personal data, so it comes in the body of a POST and never in the URL (API-04).</summary>
public sealed record SuggestLoginQuery(string FullName);
