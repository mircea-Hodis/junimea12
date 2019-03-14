using System;
using System.Net;
using System.Text;
using AuthWebApi.Auth;
using AuthWebApi.Extensions;
using AuthWebApi.Helpers;
using AuthWebApi.IRepository;
using AuthWebApi.IUploadHelpers;
using AuthWebApi.IValidators;
using AuthWebApi.Validators;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using AuthWebApi.UploadHelpers;
using AuthWebApi.Authorize;
using AuthWebApi.DataContexts;
using DataAccessLayer.IMySqlRepos;
using DataAccessLayer.IRepository;
using DataAccessLayer.MsSqlRepos;
using DataAccessLayer.MySqlRepos;
using DataModelLayer.Models;
using DataModelLayer.Models.Entities;

namespace AuthWebApi
{
    public class Startup
    {
        private const string SecretKey = "iNivDmHLpUA223sqsfhqGbMRdRj1PVkH"; // todo: get this from somewhere secure
        private readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        public Startup(
            IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AddDbContexts(services);

            services.AddSingleton<IJwtFactory, JwtFactory>();

            RegisterConfigurationBuilder(services);

            AddBuilder(services);

            AddTransients(services);

            ConfigureJwtSettings(services);

            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

            services.AddAutoMapper();
            services.AddMvc().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());
        }

        private void AddMsSqlDbContext(IServiceCollection services)
        {
            services.AddDbContext<MsSqlUserDbContext>(
                options =>
                    options.UseSqlServer(Configuration.GetConnectionString("UserAuthConnection"),
                        b => b.MigrationsAssembly("AuthWebApi")));
        }

        private void AddMySqlDbContenxt(IServiceCollection services)
        {
            services.AddDbContext<MysqlDbContext>(
                options =>
                    options.UseMySQL(Configuration.GetConnectionString("MysqlConnection"),
                        b => b.MigrationsAssembly("AuthWebApi")));
        }

        private void RegisterConfigurationBuilder(IServiceCollection services)
        {
            services.Configure<FacebookAuthSettings>(Configuration.GetSection(nameof(FacebookAuthSettings)));
            services.Configure<ConnectionInfo>(Configuration.GetSection("ConnectionStrings"));
        }

        private void AddTransients(IServiceCollection services)
        {
            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPostRepository, PostRepository>();
            services.AddTransient<IPostValidator, PostValidator>();
            services.AddTransient<IPostFilesUploadHelper, PostFilesUploadHelper>();
            services.AddTransient<IFilesRepository, FilesRepository>();
            services.AddTransient<IAuthorizationHelper, AuthorizationHelper>();
            services.AddTransient<ICommentRepository, CommentRepository>();
            services.AddTransient<ICommentFilesUploader, CommentFilesUploader>();
            services.AddTransient<IRoleCheckRepository, RoleCheckRepository>();
            services.AddTransient<IUserManagementRepository, UserManagementRepository>();
            services.AddTransient<IUserCommonDataRepository, UserCommonDataRepository>();
            services.AddTransient<ITicketsRepository, TicketsRepository>();
            services.AddScoped<RoleManager<IdentityRole>>();
        }

        private void ConfigureJwtSettings(IServiceCollection services)
        {
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = CreateTokenValidationParameters(jwtAppSettingOptions);

            AddAuthentification(services, tokenValidationParameters, jwtAppSettingOptions);
        }

        private void AddAuthentification(
            IServiceCollection services,
            TokenValidationParameters tokenValidationParameters,
            IConfigurationSection jwtAppSettingOptions)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            });

            AddAuthorization(services);
        }

        private void AddAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "ApiUser",
                    policy =>
                        policy.RequireClaim(
                            Constants.Strings.JwtClaimIdentifiers.Rol,
                            Constants.Strings.JwtClaims.ApiAccess));
                options.AddPolicy(
                    "ApiAdmin",
                    policy =>
                        policy.RequireClaim(
                            Constants.Strings.JwtClaimIdentifiers.Rol,
                            Constants.Strings.JwtClaims.Admin,
                            Constants.Strings.JwtClaims.SuperAdmin,
                            Constants.Strings.JwtClaims.ApiAccess));
                options.AddPolicy(
                    "ApiSuperAdmin",
                    policy =>
                    {
                        policy.RequireClaim(
                            Constants.Strings.JwtClaimIdentifiers.Rol);
                    });
            });
        }

        private TokenValidationParameters CreateTokenValidationParameters(IConfigurationSection jwtAppSettingOptions)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        private void AddBuilder(IServiceCollection services)
        {
            // ReSharper disable once ComplexConditionExpression
            var builder = services.AddIdentityCore<AppUser>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            });

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddEntityFrameworkStores<MsSqlUserDbContext>().AddDefaultTokenProviders();
        }

        private void AddDbContexts(IServiceCollection services)
        {
            AddMsSqlDbContext(services);

            AddMySqlDbContenxt(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            UseDevelopmentExeptionPage(app, env);
            UserExeptionHandler(app);
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }

        private void UseDevelopmentExeptionPage(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }

        private void UserExeptionHandler(IApplicationBuilder app)
        {
            app.UseExceptionHandler(
                builder =>
                {
                    builder.Run(
                        async context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                            var error = context.Features.Get<IExceptionHandlerFeature>();
                            if (error != null)
                            {
                                context.Response.AddApplicationError(error.Error.Message);
                                await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                            }
                        });
                });
        }
    }
}
