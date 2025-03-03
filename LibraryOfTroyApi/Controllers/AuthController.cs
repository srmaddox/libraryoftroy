using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using LibraryOfTroyApi.Data;
using LibraryOfTroyApi.DTOs;
using LibraryOfTroyApi.Model;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LibraryOfTroyApi.Controllers;

[Route ( "api/[controller]" )]
[ApiController]
public class AuthController : ControllerBase {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly LibraryDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController (
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        LibraryDbContext context,
        ILogger<AuthController> logger ) {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }
    [HttpGet ( "test" )]
    public IActionResult TestAuth ( ) {
        return Ok ( new { message = "Auth controller is accessible to anyone" } );
    }

    // Add a user info endpoint to AuthController
    [HttpGet ( "userinfo" )]
    [Authorize]
    public async Task<IActionResult> GetUserInfo ( ) {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if ( userId == null ) {
            return Unauthorized ( );
        }

        var user = await _userManager.FindByIdAsync(userId);
        if ( user == null ) {
            return NotFound ( "User not found" );
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok ( new {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CustomerId = user.CustomerId,
            Roles = roles
        } );
    }

    [HttpPost ( "register" )]
    public async Task<IActionResult> Register ( [FromBody] RegisterRequest request ) {
        if ( !ModelState.IsValid ) {
            return BadRequest ( ModelState );
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.UserName,
            DisplayName = request.DisplayName ?? request.UserName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if ( result.Succeeded ) {
            // Create a corresponding Customer record
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserName = request.UserName
            };

            _context.Customers.Add ( customer );
            await _context.SaveChangesAsync ( );

            // Link Customer to ApplicationUser
            user.CustomerId = customer.Id;
            await _userManager.UpdateAsync ( user );

            // Add to Customer role by default
            await _userManager.AddToRoleAsync ( user, "Customer" );

            _logger.LogInformation ( $"User {user.UserName} created a new account." );
            return Ok ( new { message = "User registered successfully." } );
        }

        foreach ( var error in result.Errors ) {
            ModelState.AddModelError ( string.Empty, error.Description );
        }

        return BadRequest ( ModelState );
    }

    [HttpPost ( "login" )]
    public async Task<IActionResult> Login ( [FromBody] LoginRequest request ) {
        if ( !ModelState.IsValid ) {
            return BadRequest ( ModelState );
        }

        var result = await _signInManager.PasswordSignInAsync(
            request.UserName,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: false);

        if ( result.Succeeded ) {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if ( user != null ) {
                var token = await GenerateJwtToken(user);
                return Ok ( token );
            }
        }

        return Unauthorized ( new { message = "Invalid login attempt." } );
    }

    private async Task<AuthResponse> GenerateJwtToken ( ApplicationUser user ) {
        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("DisplayName", user.DisplayName)
        };

        foreach ( var role in userRoles ) {
            claims.Add ( new Claim ( ClaimTypes.Role, role ) );
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(7);

        var token = new JwtSecurityToken(
            _configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"],
            claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();

        return new AuthResponse {
            Token = tokenHandler.WriteToken ( token ),
            Expiration = expires,
            User = new AuthResponse.UserInfo {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                Roles = userRoles.ToList ( )
            }
        };
    }

    [HttpPost ( "makeLibrarian" )]
    public async Task<IActionResult> MakeLibrarian ( [FromBody] string userName ) {
        var user = await _userManager.FindByNameAsync(userName);
        if ( user == null ) {
            return NotFound ( $"User '{userName}' not found." );
        }

        // Ensure Librarian role exists
        if ( !await _roleManager.RoleExistsAsync ( "Librarian" ) ) {
            await _roleManager.CreateAsync ( new IdentityRole ( "Librarian" ) );
        }

        // Add user to Librarian role
        await _userManager.AddToRoleAsync ( user, "Librarian" );

        // Update user flag
        user.IsLibrarian = true;
        await _userManager.UpdateAsync ( user );

        return Ok ( $"User '{userName}' has been made a librarian." );
    }
}