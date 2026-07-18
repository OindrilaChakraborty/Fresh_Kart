using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAPI.DAL
{
    [Table("T_Product_Mst")]
    public class TProductMst
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int prod_id { get; set; }
        public string prod_name { get; set; }
        public int cat_id { get; set; }
        public DateTime? mfg_date { get; set; }
        public DateTime? exp_date { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
        public bool is_Active { get; set; }

        [ForeignKey("cat_id")]
        public virtual TCategoryMst TCategoryMst { get; set; }

        public virtual ICollection<TOrderMst> TOrderMsts { get; set; }
        public virtual ICollection<TOrderProductMapping> TOrderProductMappings { get; set; }
    }
}
