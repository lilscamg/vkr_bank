namespace vkr_bank.Models
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string BirthTime { get; set; }
        public string Email { get; set; }
        public string Passport { get; set; }

        // дополнительные сведения
        public bool hasFamily { get; set; }
        public bool hasChildren { get; set; }
        public bool hasCar { get; set; }
        public bool hasHigherEducation { get; set; }
        public bool hasCreditInAnotherBank { get; set; }
    }
}
