﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Forum.BLL.Services;
using Forum.BLL.Settings;
using System.Text;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using Forum.DAL.Models;
using Forum.DAL.Repositories;
using Forum.Shared.Contracts;
using Microsoft.CognitiveServices.Speech;
#if DEBUG
using Azure;
using Azure.AI.Vision.Common;
using Azure.Storage.Blobs;
using Azure.Storage;
#endif

namespace Forum
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment _env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var jwtSettings = new JwtSettings();
            Configuration.Bind(nameof(jwtSettings), jwtSettings);

            services.AddSingleton(jwtSettings);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ValidateLifetime = true
            };
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x => {
                x.SaveToken = true;
                x.TokenValidationParameters = tokenValidationParameters;
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // если запрос направлен хабу
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments(ChatClient.HUBURL)))
                        {
                            // получаем токен из строки запроса
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSingleton(tokenValidationParameters);

            if (_env.IsProduction())
            {
                services.AddDbContext<ForumAppDbContext>(options =>
                    options.UseSqlServer(Environment.GetEnvironmentVariable("defaultConnection")));
                services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<ForumAppDbContext>();
            }
            else
            {
                services.AddDbContext<ForumAppDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection2")));
                services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<ForumAppDbContext>();
            }


            services.AddScoped<IThreadRepository, ThreadRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IComentRepository, ComentRepository>();
            services.AddScoped<IVoteRepository, VoteRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ICalendarRepository, CalendarRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ICognitiveService, CognitiveService>();
            services.AddSingleton<SpeechConfig>(sp =>
            {
                var speechToTextKey = Environment.GetEnvironmentVariable("azureTextToSpeechKey");
                var azureCognitiveRegion = Environment.GetEnvironmentVariable("azureCognitiveRegion");
                return SpeechConfig.FromSubscription(speechToTextKey, azureCognitiveRegion);
            });
#if DEBUG
            services.AddSingleton<VisionServiceOptions>(sp =>
            {
                var visionKey = Environment.GetEnvironmentVariable("visionKey");
                var visionEndpoint = Environment.GetEnvironmentVariable("visionEndpoint");
                var keyCred = new AzureKeyCredential(visionKey);
                return new VisionServiceOptions(visionEndpoint, keyCred);
            });
            services.AddSingleton<BlobServiceClient>(sp =>
            {
                var accountName = Environment.GetEnvironmentVariable("blobName");
                var accountKey = Environment.GetEnvironmentVariable("blobKey");
                StorageSharedKeyCredential sharedKeyCredential =
                    new StorageSharedKeyCredential(accountName, accountKey);

                string blobUri = "https://" + accountName + ".blob.core.windows.net";
                return new BlobServiceClient(new Uri(blobUri), sharedKeyCredential);
            });
#endif
            services.AddScoped<IThreadService, ThreadService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IComentService, ComentService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUserService, UserService>();
#if DEBUG
            services.AddScoped<IImageHostService, BlobHostService>();
#else
            services.AddScoped<IImageHostService, ImageHostService>();
#endif
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddSingleton<IUriService>(provider => {
                var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/");
                return new UriService(absoluteUri);
            });

            services.AddHttpClient("image_store", c =>
            {
                c.BaseAddress = new Uri("https://api.imgbb.com/1/upload?key=" +
                    Environment.GetEnvironmentVariable("IMGBB_API_KEY"));
            });

            services.AddHttpClient("denoiser", c =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("denoiserUri"));
            });

            services.AddAutoMapper(typeof(Startup));


            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new OpenApiInfo { Title = "Forum API", Version = "v1" });
                x.ExampleFilters();
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[0] }
                };

                x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                x.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {new OpenApiSecurityScheme{Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                    }, new List<string>() }
                });
            });
            services.AddSwaggerExamplesFromAssemblyOf<Startup>();
            services.AddCors(o => o.AddPolicy("Policy", builder => {
                builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
            }));
            services.AddRazorPages();
            services.AddControllers();
			services.AddControllersWithViews();
			services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            var swaggerOptions = new Forum.Option.SwaggerOptions();
            Configuration.GetSection(nameof(Option.SwaggerOptions)).Bind(swaggerOptions);

            app.UseSwagger(option =>
            {
                option.RouteTemplate = swaggerOptions.JsonRoute;
            });
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint(swaggerOptions.UIEndpoint, swaggerOptions.Description);
            });

#if !DEBUG
            app.UseHttpsRedirection();
#endif
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("Policy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
				endpoints.MapControllerRoute(
					name: "default",
                    pattern: "{controller=swagger}/{action=index}/{id?}");
				endpoints.MapFallbackToFile("index.html");
                endpoints.MapHub<Hubs.ChatHub>(ChatClient.HUBURL);
            });
        }
    }
}
