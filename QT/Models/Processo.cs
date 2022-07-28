using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.Models
{
    [Table("processos")]
    public class Processo
    {
        [Key]
        [Column("codigo")]
        public int Codigo { get; set; }
        [Column("nome")]
        public string Nome { get; set; }
    }
}
