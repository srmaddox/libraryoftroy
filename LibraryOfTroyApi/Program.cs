using LibraryOfTroyApi.Data;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Reflection;
using LibraryOfTroyApi.DTOs;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using LibraryOfTroyApi.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LibraryOfTroyApi;
[System.Diagnostics.CodeAnalysis.SuppressMessage ( "DocumentationHeader", "ClassDocumentationHeader:The class must have a documentation header.", Justification = "<Not documenting Program boilerplate>" )]
public class Program {
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0301:Closure Allocation Source", Justification = "<Not documenting Program boilerplate>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "DocumentationHeader", "MethodDocumentationHeader:The method must have a documentation header.", Justification = "<Not documenting Program boilerplate>" )]
    public static void Main ( string [] args ) {
        string logFilePath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Documents\TroyLog.txt");
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers ( ).AddNewtonsoftJson ( options => {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        } );

        builder.Services.AddEndpointsApiExplorer ( );
        builder.Services.AddSwaggerGen ( options => {
            options.SwaggerDoc ( "v1", new OpenApiInfo {
                Title = "Library of Troy API",
                Version = "v1",
                Description = "An API for managing a library's books and customer reviews"
            } );

            options.AddSecurityDefinition ( "Bearer", new OpenApiSecurityScheme {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
            } );

            options.AddSecurityRequirement ( new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            } );

            try {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if ( File.Exists ( xmlPath ) ) {
                    options.IncludeXmlComments ( xmlPath );
                }
            } catch ( Exception ex ) {
                Debug.WriteLine ( $"Exception handled during XML comment inclusion into swagger, XMLComments will not be used for swagger generation:\n{ex}" );
            }

            options.EnableAnnotations ( );
        } );

        builder.Services.AddMemoryCache ( );

        builder.Services.AddDbContext<LibraryDbContext> ( options =>
            options.UseSqlServer ( builder.Configuration.GetConnectionString ( "DefaultConnection" ) ) );

        // Add Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole> ( options => {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
        } )
            .AddEntityFrameworkStores<LibraryDbContext> ( )
            .AddDefaultTokenProviders ( );

        builder.Services.AddAuthentication ( options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        } )
            .AddJwtBearer ( options => {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration [ "JWT:ValidIssuer" ],
                    ValidAudience = builder.Configuration [ "JWT:ValidAudience" ],
                    IssuerSigningKey = new SymmetricSecurityKey (
                        Encoding.UTF8.GetBytes ( builder.Configuration [ "JWT:SecretKey" ] ?? "DefaultSecretKeyWith32CharactersLong" ) )
                };
            } );

        builder.Services.AddLogging ( logging => {
            logging.ClearProviders ( );
            logging.AddEventLog ( options => {
                options.SourceName = "LibraryOfTroyApi";
                options.LogName = "LibraryOfTroy";
            } );
            logging.AddConsole ( options => { } );
            logging.AddDebug ( );
        } );

        builder.Services.AddCors ( options => {
            options.AddPolicy ( "AllowAngularDevClient",
                builder => builder
                    .AllowAnyOrigin ( )
                    .AllowAnyMethod ( )
                    .AllowAnyHeader ( ) );
        } );

        WebApplication app = builder.Build();
        app.UseSwagger ( options => {
            options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        } );

        app.UseSwaggerUI ( options => {
            options.SwaggerEndpoint ( "/api-docs/v1/swagger.json", "Library of Troy API v1" );
            options.RoutePrefix = "api-docs";
            options.DocumentTitle = "Library of Troy API Documentation";
            options.DefaultModelsExpandDepth ( 2 );
            options.DefaultModelRendering ( Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model );
            options.DocExpansion ( Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List );
            options.EnableDeepLinking ( );
            options.DisplayRequestDuration ( );
        } );

        ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation ( "LibraryOfTroyApi application has started and is logging to Event Viewer!" );

        using ( var scope = app.Services.CreateScope ( ) ) {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

            CreateDefaultRolesAndAdmin ( roleManager, userManager, dbContext, logger ).Wait ( );
        }

        if ( app.Environment.IsDevelopment ( ) ) {
            app.MapOpenApi ( );
        }

        app.UseCors ( "AllowAngularDevClient" );
        app.UseHttpsRedirection ( );
        app.UseAuthentication ( );
        app.UseAuthorization ( );

        app.MapControllers ( );
        app.Run ( );
    }

    private static async Task CreateDefaultRolesAndAdmin (
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        LibraryDbContext dbContext,
        ILogger<Program> logger ) {
        string[] roleNames = { "Admin", "Librarian", "Customer" };
        foreach ( var roleName in roleNames ) {
            if ( !await roleManager.RoleExistsAsync ( roleName ) ) {
                await roleManager.CreateAsync ( new IdentityRole ( roleName ) );
                logger.LogInformation ( $"Created role: {roleName}" );
            }
        }

        var adminEmail = "admin@libraryoftroy.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if ( adminUser == null ) {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "System Administrator",
                IsLibrarian = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if ( result.Succeeded ) {
                await userManager.AddToRolesAsync ( admin, new [] { "Admin", "Librarian" } );

                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    UserName = adminEmail
                };

                dbContext.Customers.Add ( customer );
                await dbContext.SaveChangesAsync ( );

                admin.CustomerId = customer.Id;
                await userManager.UpdateAsync ( admin );

                logger.LogInformation ( "Admin user created successfully" );
            } else {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError ( $"Failed to create admin user: {errors}" );
            }
        }
    }
}
