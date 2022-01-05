using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neo4jClient;
using API.Security;
using API.Interfaces;
using API.Services;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SendGrid;
using API.Repositories;

namespace API
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
            services.AddCors(options => 
            {
                options.AddPolicy(name: "MyPolicy",
                builder =>{
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            services.AddControllers();
            
            var key = Encoding.ASCII.GetBytes(Configuration["TokenConfigurations:Secret"]);
            var time  = int.Parse(Configuration["TokenConfigurations:Hours"]);
            services.AddAuthentication(x =>{
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true //se der merda apaga aqui
                };
            });


            //database DI
            var uri = Configuration["Neo4jConnection:Uri"];
            var user = Configuration["Neo4jConnection:User"];
            var password = Configuration["Neo4jConnection:Password"];
            var neo4jConnection = new GraphClient(new Uri(uri), user, password);
            neo4jConnection.ConnectAsync();
            services.AddSingleton<IGraphClient>(neo4jConnection);
            var locationRepository = new LocationRepository(neo4jConnection);
            services.AddSingleton<ILocation>(locationRepository);

            //SendGrid Client
            var apiKey = Configuration["SendGrid:SENDGRID_API_KEY"];
            var client = new SendGridClient(apiKey);
            services.AddSingleton<ISendGridClient>(client);
            var emailServices = new EmailServices(client);
            services.AddSingleton<IEmailServices>(emailServices);

            //PublicationRepository DI
            var publicationRepository = new PublicationRepository(neo4jConnection);
            services.AddSingleton<IPublication>(publicationRepository);
            //UserRepository DI
            var userRepository = new UserRepository(neo4jConnection: neo4jConnection, locationRepository: locationRepository, publicationRepository: publicationRepository);
            services.AddSingleton<IUser>(userRepository);

            //security  DI
            var security = new SafetyValidations();
            services.AddSingleton<ISecurity>(security);

            //UserServices DI
            var userServices = new UserServices(userRepository: userRepository, security: security);
            services.AddSingleton<IUserServices>(userServices);

            //publicationServices DI
            var publicationService = new PublicationServices(publicationRepository, userServices: userServices);
            services.AddSingleton<IPublicationServices>(publicationService);

            //AvaliationRepository DI
            var evaluationRepository = new EvaluationRepository(neo4jConnection);
            services.AddSingleton<IEvaluation>(evaluationRepository);

            //AvaliationServices DI
            var evaluationServices = new EvaluationServices(evaluationRepository: evaluationRepository, publicationServices: publicationService, userServices: userServices);
            services.AddSingleton<IEvaluationServices>(evaluationServices);

            //HomeRepository DI
            var homeRepository = new HomeRepository(neo4jConnection);
            services.AddSingleton<IHome>(homeRepository);

            //HomeService DI
            var HomeServices = new HomeServices(homeRepository: homeRepository, publicationServices: publicationService, userServices: userServices, evaluationServices: evaluationServices);
            services.AddSingleton<IHomeServices>(HomeServices);

            //TokenServices DI
            var tokenServices = new TokenServices(key: key, time: time);
            services.AddSingleton<ITokenServices>(tokenServices);
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
            app.UseCors("MyPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Use(async (context, next) =>
                {
                    context.Response.OnStarting(() =>
                        {
                            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                            return Task.FromResult(0);
                        });
                    await next();
                }
            );
        }
    }
}
