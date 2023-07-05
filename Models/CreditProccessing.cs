using System.ComponentModel.DataAnnotations;

namespace vkr_bank.Models
{
    public class CreditProccessing
    {
        [Key]
        public int CreditId { get; set; }
        public string request_ui_str { get; set; }
        public string request_cr_str { get; set; }
        public string request_oi_str { get; set; }
    }
}
