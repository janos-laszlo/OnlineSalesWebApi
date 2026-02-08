using CSharpFunctionalExtensions;
using UserIdentity.Entities;

namespace UserIdentity.Commands;

public interface IGetUserInfoCommand
{
    Task<Result<UserInfoDto>> Execute(int userId, CancellationToken cancellationToken);
}

internal sealed class GetUserInfoCommand(UserIdentityDbContext dbContext) : IGetUserInfoCommand
{
    public async Task<Result<UserInfoDto>> Execute(int userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserInfoDto>("User not found");
        }

        string? firstName = null;
        string? lastName = null;
        string? cui = null;
        string? companyName = null;
        string? registrationNumber = null;
        string? address = null;
        string? county = null;
        string? locality = null;
        IReadOnlyList<string>? phoneNumbers = null;
        user.Profile.Match(
            regular =>
            {
                firstName = regular.FirstName;
                lastName = regular.LastName;
                phoneNumbers = regular.PhoneNumbers;
            },
            dealer =>
            {
                cui = dealer.Cui;
                companyName = dealer.CompanyName;
                registrationNumber = dealer.RegistrationNumber;
                address = dealer.Address;
                county = dealer.County;
                locality = dealer.Locality;
                phoneNumbers = dealer.PhoneNumbers;
            });

        return new UserInfoDto(
            user.Id,
            user.Email,
            user.CreatedAt,
            user.EmailConfirmed,
            user.Profile is DealerProfile,
            firstName,
            lastName,
            cui,
            companyName,
            registrationNumber,
            address,
            county,
            locality,
            phoneNumbers);
    }
}
