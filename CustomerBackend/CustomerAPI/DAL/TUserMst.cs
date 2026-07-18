using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAPI.DAL
{
    [Table("T_User_Mst")]
    public class TUserMst
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int user_id { get; set; }
        public int login_id { get; set; }
        public string user_name { get; set; }
        public string city { get; set; }
        public string pincode { get; set; }
        public string gender { get; set; }
        public bool is_Active { get; set; }

        [ForeignKey("login_id")]
        public virtual TLoginMst TLoginMst { get; set; }
    }
}
