using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

public class RegisterRequest {
    [Required]
    [EmailAddress]
    [JsonProperty ( "userName" )]
    public required string UserName { get; set; }

    [Required]
    [JsonProperty ( "password" )]
    public required string Password { get; set; }

    [Required]
    [Compare ( "Password" )]
    [JsonProperty ( "confirmPassword" )]
    public required string ConfirmPassword { get; set; }

    [JsonProperty ( "displayName" )]
    public string? DisplayName { get; set; }
}

public class LoginRequest {
    [Required]
    [JsonProperty ( "userName" )]
    public required string UserName { get; set; }

    [Required]
    [JsonProperty ( "password" )]
    public required string Password { get; set; }

    [JsonProperty ( "rememberMe" )]
    public bool RememberMe { get; set; } = false;
}

public class AuthResponse {
    [JsonProperty ( "token" )]
    public required string Token { get; set; }

    [JsonProperty ( "expiration" )]
    public DateTime Expiration { get; set; }

    [JsonProperty ( "user" )]
    public UserInfo User { get; set; }

    public AuthResponse ( ) {
        User = new UserInfo {
            Id = string.Empty,
            UserName = string.Empty,
            Email = string.Empty,
            Roles = new List<string> ( )
        };
    }

    public class UserInfo {
        [JsonProperty ( "id" )]
        public string Id { get; set; } = string.Empty;

        [JsonProperty ( "userName" )]
        public string UserName { get; set; } = string.Empty;

        [JsonProperty ( "displayName" )]
        public string? DisplayName { get; set; }

        [JsonProperty ( "email" )]
        public string Email { get; set; } = string.Empty;

        [JsonProperty ( "roles" )]
        public List<string> Roles { get; set; } = new List<string> ( );
    }
}