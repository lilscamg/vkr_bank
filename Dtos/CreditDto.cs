using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace vkr_bank.Dtos
{
    public class CreditDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int CreditType { get; set; }
        [Required, Remote(action: "CheckNegativeAmount", controller: "Credit", ErrorMessage = "Только положительные числа от 20000 до 5000000")]
        public double CreditAmount { get; set; }
        [Required, Remote(action: "CheckNegativeTerm", controller: "Credit", ErrorMessage = "Только положительные числа от 12 до 60")]
        public int CreditTerm { get; set; }
        [Required]
        public double MonthlyPayment { get; set; }
        [Required]
        public bool isDifferentiated { get; set; }
    }
}
