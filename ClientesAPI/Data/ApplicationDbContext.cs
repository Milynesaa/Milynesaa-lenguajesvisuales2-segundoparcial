using Microsoft.EntityFrameworkCore;
using ClientesAPI.Models;

namespace ClientesAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ArchivoCliente> ArchivosClientes { get; set; }
        public DbSet<LogApi> LogsApi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.CI);
                entity.Property(e => e.CI).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombres).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Direccion).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Telefono).IsRequired().HasMaxLength(20);
            });

            // Configuración de ArchivoCliente
            modelBuilder.Entity<ArchivoCliente>(entity =>
            {
                entity.HasKey(e => e.IdArchivo);
                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Archivos)
                      .HasForeignKey(e => e.CICliente)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de LogApi
            modelBuilder.Entity<LogApi>(entity =>
            {
                entity.HasKey(e => e.IdLog);
            });
        }
    }
}