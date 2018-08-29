using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace eftest
{
    public class EfTestDbContext : DbContext
    {
        private static LoggerFactory loggerFactory;

        static EfTestDbContext() {
            loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new EFLoggingProvider());
        }

        public DbSet<InternalOrder> InternalOrders { get; set; }
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<OrderTime> OrderTimes { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Filename=test.db")
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

                // builder.Entity("InternalOrder", b =>
                // {
                //     b.HasOne("OrderTime", "OrderTime")
                //         .WithOne()
                //         ;
                // });
        }
    }
}