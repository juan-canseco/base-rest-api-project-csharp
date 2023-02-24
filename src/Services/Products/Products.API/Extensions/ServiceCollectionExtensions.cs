using Application.Shared.Pipes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Products.Application;
using Products.Application.Constants;
using Products.Application.Domain;
using Products.Application.Infrastructure.Authentication;
using Products.Application.Infrastructure.Persistence;
using Products.Application.Interfaces.Authentication;
using Products.Application.Interfaces.Persistence;
using Products.Application.Requirements;
using Products.Application.Shared;
using System.Text;
using System.Text.Json;
using Utils.Time;

namespace Products.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ApplicationConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            services.Configure<DapperConfig>(options => options.ConnectionString = connectionString);
            services.AddScoped<IDapperContext, DapperContext>();
        }

        public static void AddPipes(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }

        public static void AddUtils(this IServiceCollection services)
        {
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddMediatR(typeof(IApplicationLayer).Assembly);
            services.AddAutoMapper(typeof(IApplicationLayer).Assembly);
            services.AddValidatorsFromAssembly(typeof(IApplicationLayer).Assembly);
        }

        public static void AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
            services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddDefaultUI()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


            services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWTSettings:Issuer"],
                    ValidAudience = configuration["JWTSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]))
                };

                options.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "application/json";
                        var responseModel = new
                        {
                            Title = "Authentication Failed",
                            Message = c.Exception.ToString(),
                            Succeeded = false
                        };
                        var response = JsonSerializer.Serialize(responseModel);
                        return c.Response.WriteAsync(response);
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var responseModel = new
                        {
                            Title = "Authorization Failed",
                            Message = "You are not Authorized",
                            Succeeded = false
                        };
                        var result = JsonSerializer.Serialize(responseModel);
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var responseModel = new
                        {
                            Title = "Authorization Failed",
                            Message = "You are not authorized to access this resource",
                            Succeeded = false
                        };
                        var result = JsonSerializer.Serialize(responseModel);
                        return context.Response.WriteAsync(result);
                    },
                };
            });
        }

        public static void AddAuthrorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                var permissions = Permissions.Factory.CreatePermissionsForModule();
                foreach (var permission in permissions)
                {
                    options.AddPolicy(permission, options =>
                    {
                        options.RequireAuthenticatedUser();
                        options.Requirements.Add(new PermissionRequirement(permission));
                    });
                }
            });
        }
    }
}
