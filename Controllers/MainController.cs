using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using vkr_bank.Helpers;
using vkr_bank.Models;

namespace vkr_bank.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Main()
        {
            return View();
        }
        [HttpGet]
        public void DecryptMessageTest(string ciphertext)
        {
            var _cryptoService = new CryptoService();
            var passphrase = "zhopa";

            Console.WriteLine($"Ciphertext: {ciphertext}");
            var message = _cryptoService.Decrypt(ciphertext, passphrase);
            Console.WriteLine($"Message: {message}");
        }
    }
}

