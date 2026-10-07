using System.ComponentModel.DataAnnotations;

namespace ControlService.API.Auth;

/// <summary>The `Auth` keys that decide how access tokens are signed and checked.
/// The keys Infrastructure reads live in the same section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Auth";

    [Range(1, 24 * 60)]
    public int AccessTokenMinutes { get; init; }

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>The base64 of at least 32 random bytes. A secret: user secrets locally.</summary>
    public string SigningKey { get; init; } = string.Empty;

    public byte[] SigningKeyBytes() => Convert.FromBase64String(SigningKey);
}
