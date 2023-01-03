using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int IdProvider { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(20)]
        public string Type { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(20)]
        public string N_Tracking { get; set; }

        [ForeignKey("IdProvider")]
        public virtual Provider Provider { get; set; }
    }
}
