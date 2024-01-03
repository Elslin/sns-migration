using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSEndPointCreation
{
    public class AppDbContext : DbContext
    {
        private string _connectionString;
        public AppDbContext(string connectionString){ this._connectionString = connectionString; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@_connectionString);
        }
        public DbSet<Device> Devices { get; set; }
    }
}
