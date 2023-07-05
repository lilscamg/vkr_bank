using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace vkr_bank.Dtos
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Введите номер телефона")]
        [Phone]
        [Remote(action: "CheckPhoneNumber", controller: "Account", ErrorMessage = "Пользователь с таким номером уже существует")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Длина пароля от 8 до 20 символов")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,20}$", ErrorMessage = "Пароль должен содержать цифры, строчные и заглавные буквы латиницы и быть длиной от 8 до 20 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Введите пароль повторно")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string PasswordRepeat { get; set; }


        // для SRP
        public int Verifier { get; set; }
        public string Salt { get; set; }   
    }
}