using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAPI.DAL
{
    [Table("T_Report_Gen_Serv")]
    public class TReportGenServ
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int report_gen_id { get; set; }
        public int ord_id { get; set; }
        public bool is_requested { get; set; }
        public bool is_Active { get; set; }
        public DateTime? requested_on { get; set; }
        public string requested_by { get; set; }
        public bool is_Failed { get; set; }
        public string report_type { get; set; }

        [ForeignKey("ord_id")]
        public virtual TOrderMst TOrderMst { get; set; }

        public virtual TNotificationMst TNotificationMst { get; set; }
    }
}
