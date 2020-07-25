﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using TeamspeakAnalytics.ts3provider;
using TeamspeakAnalytics.hosting.Helper;
using TeamspeakAnalytics.hosting.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using TeamspeakAnalytics.database.mssql;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TeamspeakAnalytics.ts3provider.TS3DataProviders;
using TeamspeakAnalytics.hosting.Jobs;

namespace TeamspeakAnalytics.hosting
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
      var tsCfgSection = Configuration.GetSection("TeamspeakConfiguration");
      services.Configure<TeamspeakConfiguration>(tsCfgSection);
      var tsCfg = tsCfgSection.Get<TeamspeakConfiguration>();

      var svCfgSection = Configuration.GetSection("ServiceConfiguration");
      services.Configure<ServiceConfiguration>(svCfgSection);
      var svCfg = svCfgSection.Get<ServiceConfiguration>();
      
      // Only used for development
      services.AddCors(o => o.AddPolicy("DevPolicy", builder =>
      {
        builder.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
      }));

      services.AddSingleton<IHostedService, AnalyticsJob>();
      services.AddSingleton<IHostedService, AggregationJobs>();

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "TeamspeakAnalytics - REST API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", 
          new OpenApiSecurityScheme
          {
            In = ParameterLocation.Header,
            Description = "Please enter JWT with Bearer into field (Like \"Bearer acfcaf22...\")",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey

          });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement { });
      });
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = svCfg.Hostname,
            ValidAudience = svCfg.Hostname,
            IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(svCfg.SecurityKey))
          };
        });

      services.AddDbContext<TS3AnalyticsDbContext>(opt =>
        opt.UseSqlServer(Configuration.GetConnectionString("ServiceDatabase"),
          b => b.MigrationsAssembly("TeamspeakAnalytics.database.mssql")));
      
      services.AddTS3Provider<LiveTS3DataProvider>(tsCfg);
      
      services.AddMvc(options => options.EnableEndpointRouting = false)
        .AddJsonOptions(settings =>
        {
          settings.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    }

    public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
    {
      //Fire up TS3DataProvider
      app.ApplicationServices.GetService<ITS3DataProvider>();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseCors("DevPolicy");

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamspeakAnalytics - REST API - V1"); });
      }

      app.UseAuthentication();
      app.UseMvc();
    }
  }
}