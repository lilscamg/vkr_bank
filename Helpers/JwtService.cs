using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vkr_bank.Models;

namespace vkr_bank.Helpers
{
    public class JwtService
    {
        private string _key = "devochka wensday super key";
        
        public string CreateToken(int id)
        {
            // ключ
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            // инфа о ключе и методе хэшерования
            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);
            // в хэдер передается credentials
            var header = new JwtHeader(credentials);
            // в нагрузку передается issuer, audience, claims, notbefore, expires
            var payload = new JwtPayload(id.ToString(), null, null, null, DateTime.Today.AddMonths(1));
            // хэдер + payload
            var securityToken = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }
       
        public JwtSecurityToken Verify(string jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);
            tokenHandler.ValidateToken(jwt, new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            }, out SecurityToken validatedToken);

            return (JwtSecurityToken)validatedToken;
        }
    }
}
