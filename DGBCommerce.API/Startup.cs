﻿using DGBCommerce.Data;
using DGBCommerce.Data.Repositories;
using DGBCommerce.Domain.Interfaces;

namespace DGBCommerce.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("DGBCommerce") ?? throw new ArgumentException("connectionString 'DBBCommerce' not set.");

            services.AddSingleton(provider => Configuration);
            services.AddScoped<IDataAccessLayer, DataAccessLayer>(_ => new DataAccessLayer(connectionString));
            services.AddScoped<IShopRepository, ShopRepository>();
        }

        public void Configure(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
