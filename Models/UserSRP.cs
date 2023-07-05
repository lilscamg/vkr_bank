using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace vkr_bank.Models
{
    public class UserSRP
    {
        [Key]
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public BigInteger Verifier { get; set; }
        public string Salt { get; set; }
        public int LoginAttempts { get; set; }
        public DateTime BanDate { get; set; }
        public int _2fa { get; set; }
        public string emailCode { get; set; }
        public DateTime? emailCodeDate { get; set; }
    }
}
