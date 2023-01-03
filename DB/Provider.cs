using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    public class Provider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public DateTime Created_at { get; set; }
        public int Status { get; set; }
        public int IdEnterprise { get; set; }

        [ForeignKey("IdEnterprise")]
        public virtual Enterprise Enterprise { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
