using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using iMasjid.Application.DTOs.Requests.Auth;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Constants;
using iMasjid.Domain.Exceptions;
using iMasjid.Infrastructure.Persistence;
using Xunit;

namespace iMasjid.SmokeTests;

public class PostgresAuthSmokeTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public PostgresAuthSmokeTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test_01_SuperAdmin_Can_SignIn_With_Password()
    {
        using var scope = _fixture.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var request = new LoginRequest
        {
            UserNameOrEmail = "superadmin",
            Password = "SuperAdmin123!"
        };

        var response = await authService.LoginAsync(request, "127.0.0.1");

        Assert.NotNull(response);
        Assert.False(response.RequiresTwoFactor);
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal("superadmin", response.UserName);

        var history = await dbContext.ImLoginHistories
            .AsNoTracking()
            .OrderByDescending(h => h.LoginTime)
            .FirstOrDefaultAsync(h => h.UserNameOrEmail == "superadmin");

        Assert.NotNull(history);
        Assert.True(history.IsSuccess);
    }

    [Fact]
    public async Task Test_02_New_User_Can_SignUp_And_SignIn()
    {
        var uniqueKey = Guid.NewGuid().ToString("N")[..8];
        var testUserName = $"user_{uniqueKey}";
        var testEmail = $"{testUserName}@imasjid.com";
        var password = "StrongPass@123";

        var registerRequest = new RegisterRequest
        {
            FirstName = "Ahmad",
            LastName = "Ali",
            UserName = testUserName,
            Email = testEmail,
            PhoneNumber = "+919876543210",
            Password = password,
            ConfirmPassword = password
        };

        Guid newUserId;
        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userResponse = await authService.RegisterAsync(registerRequest);

            Assert.NotNull(userResponse);
            Assert.NotEqual(Guid.Empty, userResponse.Id);
            Assert.Equal(testUserName, userResponse.UserName);
            Assert.Equal(testEmail, userResponse.Email);
            Assert.Contains(userResponse.Roles, r => r.Equals("Member", StringComparison.OrdinalIgnoreCase));
            newUserId = userResponse.Id;
        }

        using (var scope = _fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbUser = await dbContext.ImUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == newUserId);
            Assert.NotNull(dbUser);
            Assert.Equal("Ahmad", dbUser.FirstName);
            Assert.False(dbUser.TwoFactorEnabled);
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            await Assert.ThrowsAsync<ConflictException>(async () =>
            {
                await authService.RegisterAsync(registerRequest);
            });
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var loginResponse = await authService.LoginAsync(new LoginRequest
            {
                UserNameOrEmail = testUserName,
                Password = password
            }, "127.0.0.1");

            Assert.NotNull(loginResponse);
            Assert.False(loginResponse.RequiresTwoFactor);
            Assert.False(string.IsNullOrWhiteSpace(loginResponse.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(loginResponse.RefreshToken));
        }
    }

    [Fact]
    public async Task Test_03_TwoFactorAuthentication_Flow()
    {
        var uniqueKey = Guid.NewGuid().ToString("N")[..8];
        var testUserName = $"twofa_{uniqueKey}";
        var testEmail = $"{testUserName}@imasjid.com";
        var password = "StrongPass@123";

        Guid userId;
        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var user = await authService.RegisterAsync(new RegisterRequest
            {
                FirstName = "TwoFa",
                LastName = "User",
                UserName = testUserName,
                Email = testEmail,
                Password = password,
                ConfirmPassword = password
            });
            userId = user.Id;
        }

        using (var scope = _fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbUser = await dbContext.ImUsers.FirstAsync(u => u.Id == userId);
            dbUser.TwoFactorEnabled = true;
            await dbContext.SaveChangesAsync();
        }

        using (var scope = _fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbUser = await dbContext.ImUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            Assert.NotNull(dbUser);
            Assert.True(dbUser.TwoFactorEnabled);
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var loginResponse = await authService.LoginAsync(new LoginRequest
            {
                UserNameOrEmail = testUserName,
                Password = password
            }, "127.0.0.1");

            Assert.True(loginResponse.RequiresTwoFactor);
            Assert.True(string.IsNullOrEmpty(loginResponse.AccessToken));
            Assert.False(string.IsNullOrEmpty(loginResponse.MaskedEmail));
        }

        var otpCode = _fixture.GetOtp(testEmail, OtpPurposes.TwoFactor);
        Guid otpId;
        using (var scope = _fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var otpRecord = await dbContext.ImUserOtps
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Purpose == OtpPurposes.TwoFactor && !o.IsUsed);

            Assert.NotNull(otpRecord);
            Assert.Equal(64, otpRecord.OtpCode.Length);
            Assert.NotEqual(otpCode, otpRecord.OtpCode);
            otpId = otpRecord.Id;
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            {
                await authService.VerifyOtpAsync(new VerifyOtpRequest
                {
                    UserNameOrEmail = testUserName,
                    Otp = "000000",
                    Purpose = OtpPurposes.TwoFactor
                }, "127.0.0.1");
            });
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            {
                await authService.VerifyOtpAsync(new VerifyOtpRequest
                {
                    UserNameOrEmail = testUserName,
                    Otp = otpCode,
                    Purpose = OtpPurposes.Login
                }, "127.0.0.1");
            });
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var verifyResponse = await authService.VerifyOtpAsync(new VerifyOtpRequest
            {
                UserNameOrEmail = testUserName,
                Otp = otpCode,
                Purpose = OtpPurposes.TwoFactor
            }, "127.0.0.1");

            Assert.NotNull(verifyResponse);
            Assert.False(verifyResponse.RequiresTwoFactor);
            Assert.False(string.IsNullOrWhiteSpace(verifyResponse.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(verifyResponse.RefreshToken));
        }

        using (var scope = _fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var usedOtp = await dbContext.ImUserOtps.AsNoTracking().FirstOrDefaultAsync(o => o.Id == otpId);
            Assert.NotNull(usedOtp);
            Assert.True(usedOtp.IsUsed);
            Assert.NotNull(usedOtp.UsedOn);
        }
    }

    [Fact]
    public async Task Test_04_Passwordless_OTP_Login_Flow()
    {
        var uniqueKey = Guid.NewGuid().ToString("N")[..8];
        var testUserName = $"otpuser_{uniqueKey}";
        var testEmail = $"{testUserName}@imasjid.com";
        var password = "StrongPass@123";

        Guid userId;
        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var user = await authService.RegisterAsync(new RegisterRequest
            {
                FirstName = "Otp",
                LastName = "Login",
                UserName = testUserName,
                Email = testEmail,
                Password = password,
                ConfirmPassword = password
            });
            userId = user.Id;
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var sendOtpResponse = await authService.SendOtpAsync(new SendOtpRequest
            {
                UserNameOrEmail = testUserName,
                Purpose = OtpPurposes.Login
            }, "127.0.0.1");

            Assert.True(sendOtpResponse.Success);
            Assert.False(string.IsNullOrEmpty(sendOtpResponse.MaskedEmail));
        }

        var otpCode = _fixture.GetOtp(testEmail, OtpPurposes.Login);
        using (var scope = _fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var otpRecord = await dbContext.ImUserOtps
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Purpose == OtpPurposes.Login && !o.IsUsed);

            Assert.NotNull(otpRecord);
            Assert.Equal(64, otpRecord.OtpCode.Length);
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var tokenResponse = await authService.VerifyOtpAsync(new VerifyOtpRequest
            {
                UserNameOrEmail = testUserName,
                Otp = otpCode,
                Purpose = OtpPurposes.Login
            }, "127.0.0.1");

            Assert.NotNull(tokenResponse);
            Assert.False(tokenResponse.RequiresTwoFactor);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.RefreshToken));
        }
    }

    [Fact]
    public async Task Test_05_Token_Refresh_And_Revocation()
    {
        string accessToken;
        string refreshToken;
        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var loginResponse = await authService.LoginAsync(new LoginRequest
            {
                UserNameOrEmail = "superadmin",
                Password = "SuperAdmin123!"
            }, "127.0.0.1");

            accessToken = loginResponse.AccessToken;
            refreshToken = loginResponse.RefreshToken;

            var user = await dbContext.ImUsers.AsNoTracking().FirstAsync(u => u.UserName == "superadmin");
            var storedToken = await dbContext.ImRefreshTokens
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedOn)
                .FirstAsync(t => t.UserId == user.Id);
            Assert.Equal(64, storedToken.Token.Length);
            Assert.NotEqual(refreshToken, storedToken.Token);
        }

        string newRefreshToken;
        string newAccessToken;
        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var refreshedTokens = await authService.RefreshTokenAsync(new RefreshTokenRequest
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, "127.0.0.1");

            Assert.NotNull(refreshedTokens);
            Assert.False(string.IsNullOrWhiteSpace(refreshedTokens.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshedTokens.RefreshToken));
            Assert.NotEqual(refreshToken, refreshedTokens.RefreshToken);
            newAccessToken = refreshedTokens.AccessToken;
            newRefreshToken = refreshedTokens.RefreshToken;
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var revokeResult = await authService.RevokeTokenAsync(new RevokeTokenRequest
            {
                RefreshToken = newRefreshToken
            }, "127.0.0.1");

            Assert.True(revokeResult);
        }

        using (var scope = _fixture.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            await Assert.ThrowsAsync<Microsoft.IdentityModel.Tokens.SecurityTokenException>(async () =>
            {
                await authService.RefreshTokenAsync(new RefreshTokenRequest
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                }, "127.0.0.1");
            });
        }
    }
}
