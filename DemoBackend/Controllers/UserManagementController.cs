using DemoBackend.Configuration;
using DemoBackend.Constants;
using DemoBackend.Data;
using DemoBackend.Helpers;
using DemoBackend.Requests;
using DemoBackend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using LoginRequest = DemoBackend.Requests.LoginRequest;
using RegisterRequest = DemoBackend.Requests.RegisterRequest;

namespace DemoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly AuthenticationSettings _authSettings;
        private readonly ClientSettings _clientSettings;
        private readonly ITokensRepository _tokensRepository;

        public UserManagementController(UserManager<UserEntity> userManager, IOptions<AuthenticationSettings> authSettings,
           IOptions<ClientSettings> clientSettings, ITokensRepository tokensRepository)
        {
            _userManager = userManager;
            _authSettings = authSettings.Value;
            _clientSettings = clientSettings.Value;
            _tokensRepository = tokensRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            try
            {
                if (!registerRequest.Password.Equals(registerRequest.ConfirmPassword))
                {
                    return BadRequest("Passwords do not match.");
                }

                if (!registerRequest.Role.Equals(ApplicationRoles.Admin) && !registerRequest.Role.Equals(ApplicationRoles.User))
                {
                    return BadRequest("You must provide either ADMIN or USER roles");
                }

                var user = new UserEntity
                {
                    UserName = registerRequest.Email,
                    Email = registerRequest.Email,
                    Age = registerRequest.Age
                };

                //create user
                var result = await _userManager.CreateAsync(user, registerRequest.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                //assign proper rolo to the user
                if (!string.IsNullOrEmpty(registerRequest.Role))
                {
                    await _userManager.AddToRoleAsync(user, registerRequest.Role);
                }

                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                var result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
                if (!result)
                {
                    return Unauthorized("Invalid email or password.");
                }

                //fetch the user role so it can be addded on claims list inside the toke
                var userRoles = await _userManager.GetRolesAsync(user);
                var role = userRoles.FirstOrDefault() ?? string.Empty;

                //generate access token and refresh token
                var (token, expiration) = TokenProviderHelper.GenerateAccessToken(_authSettings, user, role.ToUpperInvariant());
                var refreshToken = TokenProviderHelper.GenerateRefreshToken(_authSettings);

                //save refresh token in the database
                var success = await _tokensRepository.UpsertUserRefreshTokenAsync(user.Id, refreshToken);
                if (!success)
                {
                    return Unauthorized("Invalid refresh token.");
                }

                return Ok(new AuthenticationResponse
                { 
                    AccessToken = token,
                    AccessTokenExpirationTime = expiration,
                    RefreshToken = refreshToken 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            try
            {
                if (!TokenProviderHelper.ValidateRefreshToken(_authSettings, refreshTokenRequest.RefreshToken))
                {
                    return Unauthorized("Invalid refresh token.");
                }

                var savedToken = await _tokensRepository.GetUserRefreshTokenAsync(refreshTokenRequest.RefreshToken);
                if (savedToken == null)
                {
                    return Unauthorized("Invalid refresh token.");
                }

                var user = await _userManager.FindByIdAsync(savedToken.UserId);
                if (user == null)
                {
                    return Unauthorized("Invalid refresh token.");
                }

                //since we are logged in we can get the role from the token
                var role = HttpContext.User.FindFirstValue(ApplicationClaims.Role);

                //generate access token and refresh token
                var (token, expiration) = TokenProviderHelper.GenerateAccessToken(_authSettings, user, role);

                return Ok(new RefreshTokenResponse
                {
                    AccessToken = token,
                    AccessTokenExpirationTime = expiration,
                });
            } 
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                //since we are logged in we can get the id from the token
                var userId = HttpContext.User.FindFirstValue(ApplicationClaims.Id);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid user.");
                }

                var success = await _tokensRepository.RemoveUserRefreshTokenAsync(userId);

                return success ? Ok("User logged out successfully.") : Unauthorized("Invalid user.");
            } 
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("send-reset-password-notification")]
        public async Task<IActionResult> SendResetPasswordNotification([FromBody] ResetPasswordNotificationRequest resetPasswordEmailRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordEmailRequest.Email);
                if (user?.Email is null)
                {
                    return BadRequest("User with this email not found.");
                }

                var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrWhiteSpace(passwordResetToken))
                {
                    return StatusCode(500, $"Internal server error");
                }

                var combinedToken = $"{passwordResetToken}|{user.Id}";
                var encryptedToken = EncryptionHelper.Encrypt(combinedToken);

                //create the reset password url
                var resetPasswordUrl = $"{_clientSettings.ClientUrl}reset-password?token={encryptedToken}";

                //send email with the reset password link

                //encrypted token should only be sent by email, this is just for testing purposes
                return Ok(encryptedToken);
            } 
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            try
            {
                var decryptedToken = EncryptionHelper.Decrypt(resetPasswordRequest.Token);
                var tokenParts = decryptedToken.Split('|');
                if (tokenParts.Length != 2)
                {
                    return BadRequest("Invalid token.");
                }

                var passwordResetToken = tokenParts[0];
                var userId = tokenParts[1];

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest("User with this email not found.");
                }

                var result = await _userManager.ResetPasswordAsync(user, passwordResetToken,
                    resetPasswordRequest.NewPassword);

                return result.Succeeded ? Ok("Password reset successfully.") : BadRequest("Password reset failed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("send-reset-email-notification")]
        public async Task<IActionResult> SendResetEmailNotification([FromBody] ResetEmailNotificationRequest resetEmailNotificationRequest)
        {
            try
            {
                var id = HttpContext.User.FindFirstValue(ApplicationClaims.Id);
                if (id is null)
                {
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user is null)
                {
                    return Unauthorized();
                }

                //create the reset password url
                var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, resetEmailNotificationRequest.NewEmail);
                var encryptedToken = EncryptionHelper.Encrypt(emailToken);

                var resetPasswordUrl = $"{_clientSettings.ClientUrl}reset-email?token={encryptedToken}";

                //send email with the reset email link

                //encrypted token should only be sent by email, this is just for testing purposes
                return Ok(encryptedToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("reset-email")]
        public async Task<IActionResult> ResetEmail([FromBody] ResetEmailReqest resetEmailReqest)
        {
            try
            {
                var id = HttpContext.User.FindFirstValue(ApplicationClaims.Id);
                if (id is null)
                {
                    return Unauthorized();
                }
                var user = await _userManager.FindByIdAsync(id);
                if (user is null)
                {
                    return Unauthorized();
                }

                var decryptedToken = EncryptionHelper.Decrypt(resetEmailReqest.Token);
                var result = await _userManager.ChangeEmailAsync(user, resetEmailReqest.NewEmail, decryptedToken);

                return result.Succeeded ? Ok("Email changed successfully.") : BadRequest("Email change failed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
