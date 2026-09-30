
using DemandTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Api.Data
{
    public class DemandContext(DbContextOptions<DemandContext> options) : DbContext(options)
    {
        // Define your DbContext properties and methods here
        public DbSet<Demand> Demands => Set<Demand>();
    }
}