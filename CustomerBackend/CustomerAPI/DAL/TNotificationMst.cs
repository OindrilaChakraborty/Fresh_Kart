using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAPI.DAL
{
    [Table("T_Notification_Mst")]
    public class TNotificationMst
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int not_id { get; set; }
        public int report_gen_id { get; set; }
        public bool is_email_sent { get; set; }
        public bool is_Active { get; set; }

        [ForeignKey("report_gen_id")]
        public virtual TReportGenServ TReportGenServ { get; set; }
    }
}
