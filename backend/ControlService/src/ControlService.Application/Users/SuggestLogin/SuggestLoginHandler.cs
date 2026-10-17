using ControlService.Application.Common;
using ControlService.Domain.Common;
using ControlService.Domain.Users;

namespace ControlService.Application.Users.SuggestLogin;

public sealed class SuggestLoginHandler(IUserRepository users) : IQueryHandler<SuggestLoginQuery, SuggestLoginResponse>
{
    // A suffix of up to five digits (USR-14) only ever cuts the base down to 25 characters (USR-15), so every
    // candidate starts with the first 25 characters of the suggestion without a number.
    private const int PrefixLength = 25;

    public async Task<Result<SuggestLoginResponse>> Handle(SuggestLoginQuery query, CancellationToken cancellationToken)
    {
        var firstChoice = LoginSuggestion.Suggest(query.FullName, []);
        if (firstChoice.Length == 0)
        {
            return Result<SuggestLoginResponse>.Success(new SuggestLoginResponse(string.Empty));
        }

        var prefix = firstChoice[..Math.Min(firstChoice.Length, PrefixLength)];
        var taken = await users.ListLoginsStartingWithAsync(prefix, cancellationToken);

        return Result<SuggestLoginResponse>.Success(new SuggestLoginResponse(LoginSuggestion.Suggest(query.FullName, taken)));
    }
}
