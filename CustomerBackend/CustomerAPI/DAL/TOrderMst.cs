using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAPI.DAL
{
    [Table("T_Order_Mst")]
    public class TOrderMst
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ord_id { get; set; }
        public DateTime ord_date { get; set; }
        public int ordered_by { get; set; }
        public string ord_status { get; set; }
        public int prod_id { get; set; }
        public int ord_quantity { get; set; }
        public decimal ord_price { get; set; }

        [ForeignKey("ordered_by")]
        public virtual TLoginMst TLoginMst { get; set; }

        [ForeignKey("prod_id")]
        public virtual TProductMst TProductMst { get; set; }

        public virtual ICollection<TOrderProductMapping> TOrderProductMappings { get; set; }
        public virtual ICollection<TReportGenServ> TReportGenServs { get; set; }
    }
}
