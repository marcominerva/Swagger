using AwesomeBackend.Authentication.Models;
using AwesomeBackend.Common.Models.Requests;
using AwesomeBackend.Common.Models.Responses;
using AwesomeBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AwesomeBackend.Controllers;

public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly JwtSettings jwtSettings;
    private readonly ILogger logger;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<JwtSettings> jwtSettingsOptions, ILogger<AuthController> logger)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        jwtSettings = jwtSettingsOptions.Value;
        this.logger = logger;
    }

    /// <summary>
    /// Sign-up a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser()
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        foreach (var error in result.Errors)
        {
            logger.LogError("Registration failed for user {UserName}", request.Email);
            ModelState.AddModelError("error", error.Description);
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Perform a login and obtain a new JWT Bearer token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var signInResult = await signInManager.PasswordSignInAsync(request.Email, request.Password, isPersistent: false, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
        {
            logger.LogWarning("Login failed for user {UserName}", request.Email);
            return BadRequest();
        }

        var user = await userManager.FindByNameAsync(request.Email);
        var userClaims = await userManager.GetClaimsAsync(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sid, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty)
            }.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)))
            .Union(userClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials: signingCredentials
            );

        var result = new AuthResponse(new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), jwtSecurityToken.ValidTo);
        return Ok(result);
    }
}
