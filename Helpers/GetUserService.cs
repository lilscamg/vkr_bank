using vkr_bank.Models;

namespace vkr_bank.Helpers
{
    public class GetUserService
    {
        private JwtService _jwtService;
        private ApplicationContext _db;

        public GetUserService()
        {
            _db= new ApplicationContext();
            _jwtService = new JwtService();
        }

        public UserSRP GetUser(string jwt)
        {
            try
            {
                var token = _jwtService.Verify(jwt);
                int Id = int.Parse(token.Issuer);
                var user = _db.UserSRPs.Where(u => u.Id == Id).FirstOrDefault();
                return user;
            }
            catch (Exception) { return null; }
        }
    }
}
