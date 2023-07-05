using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace vkr_bank.Dtos
{
    public class UserInfoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        public string SecondName { get; set; }

        public string ThirdName { get; set; }

        [Required(ErrorMessage = "Введите дату рождения")]
        public string BirthTime { get; set; }

        [RegularExpression(@"^[a-z0-9!#$%&'*+=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$", ErrorMessage = "Неверный формат эл. почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите серию и номер паспорта")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Неверно введены данные паспорта")]
        [Remote(action: "CheckPassport", controller: "Account", ErrorMessage = "Пользователь с таким паспортом уже существует")]
        public string Passport { get; set; }
    }
}
