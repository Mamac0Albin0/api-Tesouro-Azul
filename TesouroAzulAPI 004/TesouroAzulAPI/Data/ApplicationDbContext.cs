using Microsoft.EntityFrameworkCore;
using TesouroAzulAPI.Models;

namespace TesouroAzulAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        public DbSet<Produto> Produtos { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Fornecedor> Fornecedores { get; set; }

        public DbSet<PedidosCompra> PedidosCompra { get; set; }

        public DbSet<ItensCompra> ItensCompra { get; set; }

        public DbSet<PedidosVenda> PedidosVenda { get; set; }

        public DbSet<ItensVenda> ItensVenda { get; set; }

        public DbSet<EstoqueProduto> EstoqueProdutos { get; set; }

        public DbSet<Lucro> Lucros { get; set; }

        public DbSet<ItensLucro> ItensLucro { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Aqui pode realizar configurações adicionais, como definir chaves primárias compostas, etc.
            // Somente em casos de criar o DB atravez da propia API
        }
    }
}
