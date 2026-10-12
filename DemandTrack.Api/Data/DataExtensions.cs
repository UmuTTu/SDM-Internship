using DemandTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Api.Data
{
    public static class DataExtensions
    {
        public static void MigrateDb(this WebApplication app)//If the table does not exist, it will create the table and apply any pending migrations to the database
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DemandContext>();
            dbContext.Database.Migrate();
        }

        public static void AddDemandDb(this WebApplicationBuilder builder)
        {
            var connString = builder.Configuration.GetConnectionString("Demand");// Con string from appsettings.json

            builder.Services.AddSqlite<DemandContext>(
                connString,
                optionsAction: options => options.AddInterceptors(new TurkishCollationInterceptor()).UseSeeding((context, _) =>
                {
                    if (!context.Set<Demand>().Any())
                    {
                        // Tablo boş olduğu için numaralar çakışmaz
                        var number = DemandNumberGenerator.GetBaseNumber(DateTime.Now);
                        context.Set<Demand>().AddRange(
                            new Demand(DemandNumberGenerator.Format(number), "Demand 1", "Description for Demand 1,", "Creator 1", "Yeni", DateOnly.FromDateTime(DateTime.Now)),
                            new Demand(DemandNumberGenerator.Format(number + 1), "Demand 2", "Description for Demand 2", "Creator 2", "Yeni", DateOnly.FromDateTime(DateTime.Now))
                        );
                        context.SaveChanges();
                    }
                })
            );
        }
    }
}
