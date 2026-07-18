using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAPI.DAL
{
    [Table("T_Login_Mst")]
    public class TLoginMst
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int login_id { get; set; }
        public string email_id { get; set; }
        public string password { get; set; }
        public string user_type { get; set; }
        public DateTime created_on { get; set; }
        public bool is_Active { get; set; }

        public virtual ICollection<TUserMst> TUserMsts { get; set; }
        public virtual ICollection<TOrderMst> TOrderMsts { get; set; }
    }
}
