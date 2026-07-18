using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAPI.DAL
{
    [Table("T_Category_Mst")]
    public class TCategoryMst
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int cat_id { get; set; }
        public string cat_name { get; set; }
        public bool is_Active { get; set; }

        public virtual ICollection<TProductMst> TProductMsts { get; set; }
    }
}
