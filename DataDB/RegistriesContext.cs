using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WeatherProject_v1.Models;

namespace WeatherProject_v1.DataDB
{
    public class RegistriesContext : DbContext
    {
        public DbSet<Register> Registros { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /*
             * Hardcoded String conection.
             * In the real world it would be loaded from a configuration file to allow modification.
             * It was ok for the purposes of this exercise.
             */
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;
                                          Initial Catalog=ITHSDatabas;
                                          Integrated Security=True;
                                          Connect Timeout=30;
                                          Encrypt=False;
                                          TrustServerCertificate=False;
                                          ApplicationIntent=ReadWrite;
                                          MultiSubnetFailover=False");
        }
    }
}
