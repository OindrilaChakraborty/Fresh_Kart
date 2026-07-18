using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAPI.DAL
{
    [Table("t_order_product_mapping")]
    public class TOrderProductMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int opm_id { get; set; }
        public int order_id { get; set; }
        public int product_id { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public bool isActive { get; set; }

        [ForeignKey("order_id")]
        public virtual TOrderMst TOrderMst { get; set; }

        [ForeignKey("product_id")]
        public virtual TProductMst TProductMst { get; set; }
    }
}
