using Microsoft.AspNetCore.Mvc;
using vkr_bank.Models;
using vkr_bank.Helpers;
using vkr_bank.Dtos;
using System.Text.Json;
using System.ComponentModel;
using Newtonsoft.Json;
using Twilio.Rest.Numbers.V2.RegulatoryCompliance.Bundle;

namespace vkr_bank.Controllers
{
    public class CreditController : Controller
    {
        private ApplicationContext db;
        private readonly JwtService _jwtService;
        private readonly GetUserService _getUserService;
        private readonly CryptoService _cryptoService;
        private readonly IKeyService _keyService;

        public CreditController(IKeyService keyService)
        {
            db = new ApplicationContext();
            _jwtService = new JwtService();
            _getUserService = new GetUserService();
            _cryptoService = new CryptoService();
            _keyService = keyService;
        }

        // вернуть представление
        [HttpGet]
        public IActionResult Credits()
        {
            var jwt = Request.Cookies["jwt"];
            var user = _getUserService.GetUser(jwt);
            if (user != null)
            {
                var user_credits = db.Credits.Where(cr => cr.UserId == user.Id)
                    .OrderByDescending(cr => cr.ApplicationDate)
                    .ToList();
                ViewBag.UserСredits = user_credits;
                ViewBag.User = user;
                return View();
            }
            return BadRequest("Ошибка. Пользователь не установлен");
        }

        // вернуть представление
        [HttpGet]
        public IActionResult Credit(int id)
        {
            var jwt = Request.Cookies["jwt"];
            var user = _getUserService.GetUser(jwt);
            if (user != null)
            {
                var сredit = db.Credits.FirstOrDefault(cr => cr.Id == id);
                ViewBag.Credit = сredit;
                ViewBag.User = user;
                return View();
            }
            return BadRequest("Ошибка. Пользователь не установлен");
        }

        // проверка суммы кредита на положительную
        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckNegativeAmount(string creditAmount)
        {
            var number = Convert.ToDouble(creditAmount);
            if (number < 0)
                return Json(false);
            return Json(true);
        }

        // проверка срока кредита на положительный
        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckNegativeTerm(string creditTerm)
        {
            var number = Convert.ToDouble(creditTerm);
            if (number < 0)
                return Json(false);
            return Json(true);
        }

        // динамическое получение списка организаций
        [HttpGet]
        public IActionResult GetOrganizations(string key)
        {
            if (key != null)
            {
                var matches = from m in db.Organizations
                              where m.Name.ToLower().Contains(key.ToLower())
                              select m;
                if (matches != null) return Json(matches);
                return null;
            }
            return null;
        }
        [HttpGet]
        public IActionResult GetAllOrganizations() => Json(db.Organizations);

        // обновить данные об организации
        [HttpPost]
        public int UpdateOrganizationInfo(string oi_enc)
        {
            // расшифровка данных
            var K_s = _keyService.K_s;
            OrganizationInfoDto request = JsonConvert.DeserializeObject<OrganizationInfoDto>(_cryptoService.Decrypt(oi_enc, K_s));

            if (request.Salary < 12000 && request.Salary < 5000000)
                return 2;

            // поиск организации и информации о ней
            var organizationInfo = db.OrganizationInfos.FirstOrDefault(oi => oi.UserId == request.UserId);
            var organization = db.Organizations.FirstOrDefault(o => o.Name == request.OrganizationName);
            if (organization == null)
                return 1;

            // если инфы об организации еще нет
            if (organizationInfo == null)
            {
                organizationInfo = new OrganizationInfo()
                {
                    UserId = request.UserId,
                    OrganizationId = organization.Id,
                    Salary = request.Salary
                };
                db.OrganizationInfos.Add(organizationInfo);
            }
            // если инфа об организации уже есть
            else
            {
                organizationInfo.OrganizationId = organization.Id;
                organizationInfo.Salary = request.Salary;
            }
            db.SaveChanges();
            return 0;
        }

        // вернуть представление
        [HttpGet]
        public IActionResult LoadOrganizationInfo()
        {
            return PartialView("~/Views/Credit/OrganizationInfo.cshtml");
        }

        // оформление кредита
        [HttpPost]
        public void CreditApply(string ui_enc, string cr_enc, string oi_enc)
        {
            

            // расшифровка данных
            var K_s = _keyService.K_s;
            var _request_ui_str = _cryptoService.Decrypt(ui_enc, K_s);
            var _request_cr_str = _cryptoService.Decrypt(cr_enc, K_s);
            var _request_oi_str = _cryptoService.Decrypt(oi_enc, K_s);
            var request_ui = JsonConvert.DeserializeObject<UserInfoDto>(_request_ui_str);
            var request_cr = JsonConvert.DeserializeObject<CreditDto>(_request_cr_str);
            var request_oi = JsonConvert.DeserializeObject<OrganizationInfoDto>(_request_oi_str);

            int status = 0;
            string statusMessage = "На рассмотрении";

            // создание объекта кредита
            var credit = new Credit()
            {
                UserId = request_cr.UserId,
                CreditType = request_cr.CreditType,
                CreditAmount = request_cr.CreditAmount,
                CreditTerm = request_cr.CreditTerm,
                MonthlyPayment = request_cr.MonthlyPayment,
                isOverdue = false,
                DebtAmount = 0,
                NextPaymentDate = DateTime.UtcNow.AddMonths(1),
                Status = status,
                StatusMessage = statusMessage,
                ApplicationDate = DateTime.UtcNow,
                isDifferentiated = request_cr.isDifferentiated,
                CreditBalance = request_cr.CreditAmount,
                NumberOfPayments = 0
            };
            db.Credits.Add(credit);
            db.SaveChanges();


            credit = db.Credits.OrderByDescending(cr => cr.Id).ToList()[0];
            var creditProccesing = new CreditProccessing()
            {
                CreditId = credit.Id,
                request_cr_str = _request_cr_str,
                request_oi_str = _request_oi_str,
                request_ui_str = _request_ui_str
            };
            db.CreditProccessings.Add(creditProccesing);
            db.SaveChanges();
        }

        // сделать платеж по кредиту
        [HttpPost]
        public bool MakePayment(string enc_string_json)
        {
            // дешифровавание
            var K_s = _keyService.K_s;
            var string_json = _cryptoService.Decrypt(enc_string_json, K_s);
            var param_obj = JsonConvert.DeserializeObject<CreditPaymentDto>(string_json);
            // поиск кредита
            var credit = db.Credits.FirstOrDefault(cr => cr.Id == param_obj.CreditId);
            // проверка на размер выплаты
            if (param_obj.PaymentAmount < credit.MonthlyPayment)
                return false;
            // увеличение числа платежей и дата следующего платежа
            credit.NumberOfPayments++;
            credit.NextPaymentDate = credit.NextPaymentDate.AddMonths(1);
            // проверка на заверешенность кредита
            if (credit.NumberOfPayments == credit.CreditTerm || param_obj.PaymentAmount >= credit.CreditBalance)
            {
                credit.Status = 100;
                credit.StatusMessage = "Кредит выплачен";
                credit.CreditBalance = 0;
                credit.isOverdue = false;
                db.SaveChanges();
                return true;
            }
            // изменение остатка и суммы платежей
            double stavka = 0;
            if (credit.CreditType == 1) stavka = 14.9;
            if (credit.CreditType == 2) stavka = 9.34;
            if (credit.CreditType == 3) stavka = 5.9;

            if (!credit.isDifferentiated) // аннуитетный
            {
                var p = Math.Round(credit.CreditBalance * ((stavka / 12) / 100), 2);
                var percent = credit.MonthlyPayment - p;
                var b = credit.MonthlyPayment - percent;
                credit.CreditBalance -= percent;
            }
            else // дифференцированный
            {
                var b = Math.Round(credit.CreditAmount / credit.CreditTerm, 2);
                credit.CreditBalance = Math.Round(credit.CreditBalance - b, 2);
                // изменение суммы платежа 
                b = credit.CreditAmount / credit.CreditTerm;
                var Sn = credit.CreditAmount - (b * credit.NumberOfPayments);
                var p = Sn * ((stavka / 100) / 12);
                credit.MonthlyPayment = Math.Round(b + p, 2);
            }
            db.SaveChanges();
            return true;
        }
    }
}