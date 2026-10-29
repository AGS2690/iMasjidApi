using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iMasjid.Api.Controllers.Base;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Requests.Auth;
using iMasjid.Application.DTOs.Responses.Auth;
using iMasjid.Application.DTOs.Responses.Users;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Exceptions;

namespace iMasjid.Api.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("signup")]
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return CreatedResponse($"/api/users/{response.Id}", response, "Account registered successfully. You can now log in.");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, GetIpAddress(), cancellationToken);
        var message = response.RequiresTwoFactor
            ? $"Two-Factor Authentication is required. An OTP has been sent to your registered email ({response.MaskedEmail})."
            : "Login successful.";
        return OkResponse(response, message);
    }

    [HttpPost("send-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OtpSentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.SendOtpAsync(request, GetIpAddress(), cancellationToken);
        return OkResponse(response, response.Message);
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.VerifyOtpAsync(request, GetIpAddress(), cancellationToken);
        return OkResponse(response, "OTP verified successfully. Login successful.");
    }

    [HttpPost("toggle-2fa")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleTwoFactor([FromBody] ToggleTwoFactorRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
            throw new UnauthorizedException("Invalid user session.");

        await _authService.ToggleTwoFactorAsync(userId, request.Enable, cancellationToken);
        return OkResponse($"Two-factor authentication has been successfully {(request.Enable ? "enabled" : "disabled")}.");
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
            throw new UnauthorizedException("Invalid user session.");

        var userDetails = await _userService.GetDetailsByIdAsync(userId, cancellationToken);
        return OkResponse(userDetails);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request, GetIpAddress(), cancellationToken);
        return OkResponse(response, "Token refreshed successfully.");
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RevokeTokenAsync(request, GetIpAddress(), cancellationToken);
        return result
            ? OkResponse("Token revoked successfully.")
            : BadRequest(ApiResponse.FailureResult("Invalid or already revoked token.", correlationId: CorrelationId));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(GetIpAddress(), cancellationToken);
        return OkResponse("Logged out successfully.");
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
            throw new UnauthorizedException("Invalid user session.");

        await _authService.ChangePasswordAsync(userId, request, cancellationToken);
        return OkResponse("Password changed successfully. Please log in again with your new credentials.");
    }
}
