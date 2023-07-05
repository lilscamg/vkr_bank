using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace vkr_bank.Dtos
{
    public class UserAuthorizeDto
    {
        [Required (ErrorMessage = "Введите номер телефона")]
        public string PhoneNumber { get; set; }

        [Required (ErrorMessage = "Введите пароль")]
        public string Password { get; set; }

        // для SRP
        public string R_c { get; set; }

        // для 2fa
        public string emailCode { get; set; }
    }
}
