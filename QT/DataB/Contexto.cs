using Microsoft.EntityFrameworkCore;
using QT.Models;

namespace QT.DataB
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options)
            : base(options) { }

        public DbSet<Documento> Documentos { get; set; }
        public DbSet<Processo> Processos { get; set; }
    }
}
