using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
           
            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
               
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning)));

            
            var builder = services.AddIdentityCore<User>(options =>
            {
                // We are configuring this just for development to allow
                // weak passwords we are currently using.  In production,
                // we need to make this strict
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

          
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
         
            builder.AddEntityFrameworkStores<DataContext>();
            
            builder.AddRoleValidator<RoleValidator<Role>>();
         
            builder.AddRoleManager<RoleManager<Role>>();
         
            builder.AddSignInManager<SignInManager<User>>();

           
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                            .GetBytes(Configuration.GetSection("SecuritySettings:Token").Value)),
                      
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

           
            services.AddAuthorization(options =>
            {
               
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("ModeratePhotoRole", policy =>
                    policy.RequireRole("Admin", "Moderator"));
                options.AddPolicy("VipOnly", policy =>
                    policy.RequireRole("VIP"));
            });

         
            services.AddMvc(options => {
                var policy = new AuthorizationPolicyBuilder()
                   
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

         
            services.AddCors();

           
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

        
            services.AddScoped<IDatingRepository, DatingRepository>();

            
            services.AddTransient<Seed>();

            services.AddAutoMapper();

         
            services.AddScoped<LogUserActivity>();
        }

      
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

           
            var builder = services.AddIdentityCore<User>(options =>
            {
               
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

           
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
           
            builder.AddEntityFrameworkStores<DataContext>();
           
            builder.AddRoleValidator<RoleValidator<Role>>();
           
            builder.AddRoleManager<RoleManager<Role>>();
          
            builder.AddSignInManager<SignInManager<User>>();

      
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                            .GetBytes(Configuration.GetSection("SecuritySettings:Token").Value)),
                      
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

          
            services.AddAuthorization(options =>
            {
              
                options.AddPolicy("RequireAdminRole", policy => 
                    policy.RequireRole("Admin"));
                options.AddPolicy("ModeratePhotoRole", policy => 
                    policy.RequireRole("Admin", "Moderator"));
                options.AddPolicy("VipOnly", policy =>
                    policy.RequireRole("VIP"));
            });

     
            services.AddMvc(options => {
                var policy = new AuthorizationPolicyBuilder()
                
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
             
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddCors();

         
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

            
            services.AddScoped<IDatingRepository, DatingRepository>();

         
            services.AddTransient<Seed>();

        
            services.AddAutoMapper();

            services.AddScoped<LogUserActivity>();
        }

    
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            Seed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
              
                app.UseExceptionHandler(builder => {
                 
                    builder.Run(async context => {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                        var error = context.Features.Get<IExceptionHandlerFeature>();

                        if (error != null)
                        {
                       
                            context.Response.AddApplicationError(error.Error.Message);

                            await context.Response.WriteAsync(error.Error.Message);                            
                        }
                    });
                });
            }

        
            seeder.SeedUsers();

            
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

    
            app.UseAuthentication();

           
            app.UseDefaultFiles();
            app.UseStaticFiles();

        
            app.UseMvc(routes => {
           
                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Fallback", action = "Index" }
                );
            });
        }
    }
}
