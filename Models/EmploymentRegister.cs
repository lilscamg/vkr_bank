using System.ComponentModel.DataAnnotations;

namespace vkr_bank.Models
{
    public class EmploymentRegister /// информация организаций о том, где кто работает
    {
        [Key]
        public int RecordId { get; set; }
        public string UserPassport { get; set; }
        public int OrganizationId { get; set; }
        public double Salary {get; set; }

        // дополнительные сведения
        public DateTime BeginningOfWork { get; set; }
    }
}
