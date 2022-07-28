
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.Models
{
    [Table("documentos")]
    public class Documento
    {
        [Key]
        [Display(Name = "Código")]
        [Column("codigo")]
        [Required(ErrorMessage = "O campo de código é obrigátorio")]
        public int Codigo { get; set; }
        [Display(Name = "Título")]
        [Column("titulo")]
        [Required(ErrorMessage = "O campo de título é obrigátorio")]
        public string Titulo { get; set; }
        [Display(Name = "Processo")]
        [Column("processoCodigo")]
        [Required(ErrorMessage = "É nescessário selecionar um processo")]
        public int ProcessoCodigo { get; set; }
        public Processo Processo { get; set; }
        [Display(Name = "Categoria")]
        [Column("categoria")]
        [Required(ErrorMessage = "O campo de categoria é obrigátorio")]
        public string Categoria { get; set; }
        [Display(Name = "Arquivo")]
        [Column("arquivo")]
        [Required(ErrorMessage = "É nescessário selecionar um arquivo")]
        public byte[] Arquivo { get; set; }
        public string NomeDoArquivo { get; set; }
    }
}
