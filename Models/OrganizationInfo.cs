using System.ComponentModel.DataAnnotations;

namespace vkr_bank.Models
{
    public class OrganizationInfo // информация пользователей о том своем месте работы
    {
        [Key]
        public int RecordId { get; set; }
        public int OrganizationId { get; set; }
        public int UserId { get; set; }
        public double Salary { get; set; }
    }
}
