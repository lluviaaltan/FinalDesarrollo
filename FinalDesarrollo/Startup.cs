using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalDesarrollo.DbModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using StructureMap;

namespace FinalDesarrollo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ctrlalumnosContext>(options => options.UseSqlServer(connectionString));

            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddMvc()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddControllersWithViews();
            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("1", new OpenApiInfo
                {
                    Title = "ALUMNOS - API",
                    Description = "API Examen Final.",
                    Contact = new OpenApiContact
                    {
                        Name = "Lluvia Altná García",
                        Url = new Uri("https://umg.edu.gt/")
                    },
                });
            });

            var container = new Container();


            container.Configure(config =>
            {
                DbContextOptions<ctrlalumnosContext> dbContextOptions = new DbContextOptionsBuilder<ctrlalumnosContext>()
                .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                .Options;

                config.For<IApplicationDbContextOptions>().Use(_ => new ApplicationDbContextOptions(dbContextOptions))
                .ContainerScoped();
                
                config.Populate(services);
            });

            return container.GetInstance<IServiceProvider>();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/swagger.json", "CONTROL ALUMNOS API");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMvc(routes =>
            {
                routes.MapRoute("LocalApi", "Api/{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute("Default", "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
