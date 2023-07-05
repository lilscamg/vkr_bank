using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Numerics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using vkr_bank.Models;
using vkr_bank.Helpers;
using vkr_bank.Dtos;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Twilio.Types;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Newtonsoft.Json;

namespace vkr_bank.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationContext db;
        //private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        private readonly CryptoService _cryptoService;
        private readonly Random _random;
        private readonly _2faService _2faService;
        private readonly IKeyService _keyService;

        // для srp общие значения
        private static int N = 2096687;
        private static int g;
        private static int k = 3; // либо H(N, g)

        // для srp остальные значения
        private static BigInteger A;
        private static BigInteger B;
        private static BigInteger verifier;
        private static string salt;
        private static string phoneNumber;
        private static string K_s;
        private static string M_s;
        

        public AccountController(IKeyService keyService)
        {
            db = new ApplicationContext();
            _jwtService = new JwtService();
            _cryptoService = new CryptoService();
            g = _cryptoService.findPrimitive(N);
            _random = new Random();
            _2faService = new _2faService();
            _keyService = keyService;
        }

        // srp регистрация
        [HttpPost]
        public IActionResult SRP_Reg(UserRegisterDto request)
        {
            string _salt = request.Salt;
            BigInteger _verifier = request.Verifier;
            UserSRP user = new UserSRP() { PhoneNumber = request.PhoneNumber, Salt = _salt, Verifier = _verifier, _2fa = 0, emailCode = "" };

            db.UserSRPs.Add(user);
            db.SaveChanges();
            return RedirectToAction("Main", "Main");
        }

        // srp авторизация
        [HttpPost]
        public IActionResult SRP_Auth(UserAuthorizeDto request)
        {
            // получение пользователя из бд
            var user = db.UserSRPs.FirstOrDefault(u => u.PhoneNumber == request.PhoneNumber);
            if (user is null)
                return BadRequest("User not found");

            // обнуления срока заморозки попыток входа
            if (user.LoginAttempts > 2)
            {
                if (DateTime.UtcNow - user.BanDate > new TimeSpan(1, 0, 0))
                    user.LoginAttempts = 0;
            }
            // проверка на брутфорс
            if (user.LoginAttempts > 2)
                return BadRequest("Too many attempts to login!");

            // сравнение R_c и R_s
            string R_c = Convert.ToString(request.R_c);
            Console.WriteLine("Пользователь отправил форму авторизации. Сравнение R:");
            Console.WriteLine($"R_c = {R_c}");
            var str = A.ToString() + M_s + K_s;
            var R_s = _cryptoService.byteArrayToBigInt(_cryptoService.getSha256(str)).ToString();
            Console.WriteLine($"R_s = {R_s}");
            // 2fa
            if (user._2fa != 0)
            {
                if (DateTime.UtcNow - user.emailCodeDate > new TimeSpan(0, 5, 0))
                    return BadRequest("2FA code expired");
                // расшифровка 2fa code
                var emailCode = _cryptoService.Decrypt(request.emailCode, K_s);
                // сравнение
                if (_cryptoService.byteArrayToBigInt(_cryptoService.getSha256(emailCode)).ToString() != user.emailCode)
                {
                    user.LoginAttempts += 1;
                    if (user.LoginAttempts > 2)
                        user.BanDate = DateTime.UtcNow;
                    db.UserSRPs.Update(user);
                    db.SaveChanges();
                    return BadRequest("2FA code is invalid");
                }
            }
            if (!R_s.Equals(R_c))
            {
                user.LoginAttempts += 1;
                if (user.LoginAttempts > 2)
                    user.BanDate = DateTime.UtcNow;
                db.UserSRPs.Update(user);
                db.SaveChanges();
                return BadRequest("Password is invalid");
            }
            else
            {
                user.LoginAttempts = 0;
                user.emailCode = "";
                user.emailCodeDate = null;
            }
            db.UserSRPs.Update(user);
            db.SaveChanges();

            // создание JWT-токена
            var token = _jwtService.CreateToken(user.Id);
            Response.Cookies.Append("jwt", token, new CookieOptions()
            {
                HttpOnly = true,  // куки файл не доступен клиентским скриптом
                Secure = true  // куки доступны только для HTTPS
            });
            return RedirectToAction("Main", "Main");
        }

        // srp авторизация
        [HttpPost]
        public string SRP_Auth_1(string _phoneNumber, BigInteger _A)
        {
            // получение соли и верификатора пароля пользователя
            phoneNumber = _phoneNumber;
            var user = db.UserSRPs.Where(u => u.PhoneNumber == phoneNumber).FirstOrDefault();
            if (user == null) return string.Empty;
            salt = user.Salt;
            verifier = user.Verifier;
            A = _A;

            while (true)
            {
                // клиенту отправляется пара из соли и вычисленного B
                var b = (int)(_random.Next(N) / 1000);
                Console.WriteLine($"b = {b}");
                B = (k * verifier + _cryptoService.ModPow(g, b, N)) % N;
                Console.WriteLine($"B = {B}");

                // вычисление скремблера
                BigInteger u = _cryptoService.byteArrayToBigInt(_cryptoService.getSha256(A.ToString() + B.ToString()));
                Console.WriteLine($"Scrambler server u = {u}");
                if (u == 0)
                    return string.Empty;
                if (u > 0)
                {
                    // подсчет ключа на сервере
                    BigInteger S_s = _cryptoService.ModPow(A * _cryptoService.ModPow(verifier, u, N), b, N);
                    Console.WriteLine("S_s = " + S_s);
                    K_s = _cryptoService.byteArrayToBigInt(_cryptoService.getSha256(S_s.ToString())).ToString();
                    Console.WriteLine("K_s = " + K_s);
                    _keyService.K_s = K_s;

                    // по факту возвращается только salt и B
                    return salt + " " + B.ToString();
                }
            }
        }

        // srp авторизация
        [HttpPost]
        public IActionResult SRP_Auth_2(string M_c)
        {
            // подсчет М
            Console.WriteLine("Вычисление M на сервере:");
            var hash_N_bin = _cryptoService.to_Bin(_cryptoService.byteArrayToBigInt(_cryptoService.getSha256(N.ToString())));
            var hash_g_bin = _cryptoService.to_Bin(_cryptoService.byteArrayToBigInt(_cryptoService.getSha256(g.ToString())));
            var xor = _cryptoService.getXor(hash_N_bin, hash_g_bin);
            var hash_I = _cryptoService.byteArrayToBigInt(_cryptoService.getSha256(phoneNumber)).ToString();
            var str = xor + hash_I + salt + A.ToString() + B.ToString() + K_s;
            // Console.WriteLine("Строка: " + str);
            M_s = _cryptoService.byteArrayToBigInt(_cryptoService.getSha256(str)).ToString();
            Console.WriteLine("M_s: " + M_s);

            // получение 2fa 
            var __2fa = db.UserSRPs.FirstOrDefault(u => u.PhoneNumber == phoneNumber)._2fa;
            var _status = 0;
            if (M_c == M_s) _status = 1;
            if (__2fa == 1)
            {
                var user = db.UserSRPs.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
                var userInfo = db.UserInfos.FirstOrDefault(ui => ui.Id == db.UserSRPs.FirstOrDefault(u => u.PhoneNumber == phoneNumber).Id);
                var _code = _2faService.GenerateCode();
                user.emailCode = _cryptoService.byteArrayToBigInt(_cryptoService.getSha256(_code)).ToString();
                user.emailCodeDate = DateTime.UtcNow;
                _2faService.SendCodeToMail(_code, userInfo.Email, userInfo.SecondName + " " + userInfo.FirstName);
                db.SaveChanges();
            }
            return Json(new { _2fa = __2fa, status = _status });
        }

        // проверка существования номера телефона в бд
        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckPhoneNumber(string phoneNumber)
        {
            if (db.UserSRPs.Any(u => u.PhoneNumber == phoneNumber))
                return Json(false);
            return Json(true);
        }

        // проверка существования паспорта в бд
        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckPassport(string passport)
        {
            if (db.UserInfos.Any(u => u.Passport == passport))
                return Json(false);
            return Json(true);
        }

        // выход из системы
        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Main", "Main");
        }

        // вернуть представление
        [HttpGet]
        public IActionResult PersonalAccount() { return View(); }

        // обновить персональные данные
        [HttpPost]
        public void UpdateUserInfo(string ui_enc)
        {
            // расшифровка данных
            var K_s = _keyService.K_s;
            UserInfoDto request = JsonConvert.DeserializeObject<UserInfoDto>(_cryptoService.Decrypt(ui_enc, K_s));
            Console.WriteLine($"Ключ шифрования сервера:\n{K_s}");
            Console.WriteLine($"Полученный сервером шифротекст:\n{ui_enc}");
            Console.WriteLine($"Результат дешифрования:\n{_cryptoService.Decrypt(ui_enc, K_s)}");

            var userInfo = db.UserInfos.FirstOrDefault(u => u.Id == request.Id);
            // если инфы о пользователе еще нет
            if (userInfo == null)
            {
                userInfo = new UserInfo();
                userInfo.Id = request.Id;
                userInfo.FirstName = request.FirstName;
                userInfo.SecondName = request.SecondName;
                if (request.ThirdName != null)
                    userInfo.ThirdName = request.ThirdName;
                else
                    userInfo.ThirdName = "";
                userInfo.BirthTime = request.BirthTime;
                if (request.Email != null)
                    userInfo.Email = request.Email;
                else
                    userInfo.Email = "";
                userInfo.Passport = request.Passport;

                db.UserInfos.Add(userInfo);
            }
            // если инфа о пользователе уже есть
            else
            {
                userInfo.FirstName = request.FirstName;
                userInfo.SecondName = request.SecondName;
                if (request.ThirdName != null)
                    userInfo.ThirdName = request.ThirdName;
                else
                    userInfo.ThirdName = "";
                userInfo.BirthTime = request.BirthTime;
                if (request.Email != null)
                    userInfo.Email = request.Email;
                else
                    userInfo.Email = "";
                userInfo.Passport = request.Passport;
            }
            db.SaveChanges();
        }

        // вернуть представление
        [HttpGet]
        public IActionResult LoadUserInfo() { return PartialView("UserInfo"); }

        // настройка 2fa
        [HttpPost]
        public int UpdateSettings(int id, int new_2fa)
        {
            var user = db.UserSRPs.FirstOrDefault(u => u.Id == id);
            var userInfo = db.UserInfos.FirstOrDefault(ui => ui.Id == id);
            if (new_2fa == 1 && (userInfo.Email == null || userInfo.Email == ""))
                return 1;
            if (new_2fa == 2)
                return 2;
            user._2fa = new_2fa;
            db.SaveChanges();
            return 0;
        }

        // вернуть представление
        [HttpGet]
        public IActionResult LoadSettings() { return PartialView("Settings"); }
    }
}
