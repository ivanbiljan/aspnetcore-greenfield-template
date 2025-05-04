using Microsoft.AspNetCore.Identity;
using NodaTime.Extensions;
using WebApi.Tests.Infrastructure;

namespace WebApi.Tests.Features.Authentication;

public sealed class LoginTests(CustomApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public Task UnknownEmail_ThrowsNotFoundException()
    {
        Send<Login.Command, Login.Response>(
                new Login.Command
                {
                    Email = "does not exist",
                    Password = ""
                }
            )
            .ShouldThrow<NotFoundException>();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task EmailNotConfirmed_ThrowsNotFoundException()
    {
        await InsertAsync(
            new User
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.UtcNow.ToLocalDateTime().Date,
                Email = "test@acme.com",
                Password = "test"
            }
        );

        Send<Login.Command, Login.Response>(
                new Login.Command
                {
                    Email = "test@acme.com",
                    Password = "test"
                }
            )
            .ShouldThrow<NotFoundException>();
    }

    [Fact]
    public async Task InvalidPassword_ThrowsNotFoundException()
    {
        await InsertAsync(
            new User
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.UtcNow.ToLocalDateTime().Date,
                Email = "test@acme.com",
                Password = "test",
                EmailConfirmedAtUtc = DateTime.UtcNow.ToInstant()
            }
        );

        Send<Login.Command, Login.Response>(
                new Login.Command
                {
                    Email = "test@acme.com",
                    Password = "test"
                }
            )
            .ShouldThrow<NotFoundException>();
    }

    [Fact]
    public async Task ConfirmedAndValidPassword_ReturnsAccessToken()
    {
        var passwordHasher = GetService<IPasswordHasher<User>>();
        await InsertAsync(
            new User
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.UtcNow.ToLocalDateTime().Date,
                Email = "test@acme.com",
                Password = passwordHasher.HashPassword(null!, "test"),
                EmailConfirmedAtUtc = DateTime.UtcNow.ToInstant()
            }
        );

        var response = await Send<Login.Command, Login.Response>(
            new Login.Command
            {
                Email = "test@acme.com",
                Password = "test"
            }
        );

        response.AccessToken.ShouldNotBeNullOrEmpty();
        response.ExpiresAtUtc.ShouldBeGreaterThan(0);
        response.RefreshToken.ShouldNotBeNullOrEmpty();