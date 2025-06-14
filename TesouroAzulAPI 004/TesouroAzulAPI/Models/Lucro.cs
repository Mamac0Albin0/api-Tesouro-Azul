using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesouroAzulAPI.Models
{
    [Table("TB_LUCRO")]
    public class Lucro
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_LUCRO { get; set; }
        [Required]
        public int ID_USUARIO_FK { get; set; }
        [Required, Column(TypeName = "date")]
        public DateTime DATA_CONTABILIZACAO_LUCRO { get; set; }
        [Column(TypeName ="decimal(10,2)")]
        public decimal VALOR_LUCRO { get; set; }

        [ForeignKey(nameof(ID_USUARIO_FK))]
        public Usuario Usuario { get; set; }
    }

    [Table("TB_ITENS_LUCRO")]
    public class ItensLucro
    {
        [Key]
        public int ID_LUCRO_FK { get; set; }
        [Required]
        public int ID_PEDIDO_VENDA_FK { get; set; }
        [Required]
        public int ID_COMPRA_FK { get; set; }

        // Referenciando FKs

        [ForeignKey(nameof(ID_LUCRO_FK))]
        public Lucro Lucro { get; set; }
        
        [ForeignKey(nameof(ID_PEDIDO_VENDA_FK))]
        public PedidosVenda PedidoVenda { get; set; }

        [ForeignKey(nameof(ID_COMPRA_FK))]
        public PedidosCompra PedidosCompra { get; set; }



    }
}
