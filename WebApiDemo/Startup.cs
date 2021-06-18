using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using DataAccess.DataConnectContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NSwag.Generation.Processors.Security;
using Microsoft.OpenApi.Models;
using NSwag;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NSwag.AspNetCore;

namespace WebApiDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var applicationOwner = Configuration.GetSection("ClientConfiguration").GetValue<string>("ApplicationOwner");
            var applicationVersion = Configuration.GetSection("ClientConfiguration").GetValue<string>("ApplicationVersion");
            var databaseSchema = Configuration.GetConnectionString("DatabaseSchema");
            var isSuer = Configuration.GetSection("Jwt").GetValue<string>("Issuer");

            services.AddMvc();
            services.AddControllers();
            services.AddDbContext<DataContext>(option =>
            {
                option.UseSqlServer(Configuration.GetConnectionString("ConnectionDbContext"),
                    sqlServerOptionsAction: sqlOptions=>{
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        
                        });                
                
            });

            //JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "JWT Token",
                    
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuers = new string[] { isSuer }, //(IEnumerable<string>)Configuration["Jwt:Issuer"],
                    ValidAudience = isSuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };

            });

            services.AddSwaggerDocument(config =>
            {
                //set env discription
                config.PostProcess = document =>
                {
                    document.Info.Version = applicationVersion;
                    document.Info.Title = "Chat - Web APIs";
                    document.Info.Description = $"Web APIs: ASP.NET 5, Owner: {applicationOwner}, Schema: {databaseSchema}";
                };


                // set to screen pass token header
                config.DocumentName = "OpenAPI 2";
                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT Token"));
                config.AddSecurity("JWT Token", Enumerable.Empty<string>(),
                    new NSwag.OpenApiSecurityScheme()
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Copy this into the value field: Bearer {token}"
                    }
                );              

                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("bearer"));
              
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();               
            }

            app.UseHttpsRedirection();
            
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseOpenApi();
            app.UseSwaggerUi3(settings => {
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    ClientId = "demo_api_swagger",

                    AppName = "Demo API - Swagger",

                };

            });           

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
           
            //app.UseMvc();
        }
    }
}
