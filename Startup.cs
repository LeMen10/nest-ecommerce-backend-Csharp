using back_end.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace back_end
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
            var key = Configuration["Jwt:Key"];
            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationSchme);
            services.AddCors(options =>
            {
                options.AddPolicy("MyAllowSpecificOrigins", builder =>
                {
                    builder.WithOrigins("http://localhost:3000", "http://localhost:3001")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddControllers();

            services.AddDbContext<web_apiContext>(option =>
            {
                //Connection string 
                option.UseSqlServer(Configuration.GetConnectionString("web_api"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "back_end", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "web_api v1"));
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("MyAllowSpecificOrigins");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}