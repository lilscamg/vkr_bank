using Newtonsoft.Json;
using vkr_bank.Dtos;
using vkr_bank.Models;

namespace vkr_bank.Helpers
{
    public class CreditService
    {
        ApplicationContext db;
        public CreditService()
        {
            db = new ApplicationContext();
        }

        public void CreditApproval(int userId)
        {
            // статусы
            /* 
            0 - ожидает подтверждения

            1 - 6 кредит не одобрен
            1 - проблема с местом работы
            2 - проблема с зп
            3 - проблема с клиентом
            4 - плохая кредитная история 

            99 - активный кредит
            100 - кредит закрыт 
            */

            // поиск кредитов пользователя, ожидающих подтверждения
            var credits = db.Credits.Where(c => (c.UserId == userId && c.Status == 0)).ToList();
            if (credits.Any())
            {
                foreach (var credit in credits)
                {
                    // если прошло 5 минут
                    if (DateTime.UtcNow - credit.ApplicationDate > new TimeSpan(0, 5, 0))
                    {
                        var cP = db.CreditProccessings.FirstOrDefault(cp => cp.CreditId == credit.Id);
                        var request_ui = JsonConvert.DeserializeObject<UserInfoDto>(cP.request_ui_str);
                        var request_cr = JsonConvert.DeserializeObject<CreditDto>(cP.request_cr_str);
                        var request_oi = JsonConvert.DeserializeObject<OrganizationInfoDto>(cP.request_oi_str);

                        int status = 99;
                        string statusMessage = "Активный кредит";

                        // проверка данных заявки
                        var employee = db.EmploymentRegisters.FirstOrDefault(e => e.UserPassport == request_ui.Passport);
                        if (employee == null)
                        {
                            status = 1;
                            statusMessage = "Место работы не подтверждено";
                        }
                        else
                        {
                            if (employee.OrganizationId != db.Organizations.FirstOrDefault(o => o.Name == request_oi.OrganizationName).Id)
                            {
                                status = 1;
                                statusMessage = "Место работы не подтверждено";
                            }
                            else
                            {
                                // если стаж работы меньше полугода
                                if (DateTime.UtcNow - employee.BeginningOfWork < new DateTime(2020, 12, 1) - new DateTime(2020, 6, 1))
                                {
                                    status = 1;
                                    statusMessage = "Стаж работы меньше полугода";
                                }
                                else
                                {
                                    // проверка заработной платы
                                    if (request_oi.Salary != employee.Salary)
                                    {
                                        status = 2;
                                        statusMessage = "Заработная плата не подтверждена";
                                    }
                                    else
                                    {
                                        // проверка суммы заема
                                        if (request_cr.MonthlyPayment > 0.4 * employee.Salary)
                                        {
                                            status = 2;
                                            statusMessage = "Заработная плата не является соответствующей платежам по кредитам";
                                        }
                                        else
                                        {
                                            // проверка возраста
                                            if (DateTime.UtcNow - DateTime.Parse(request_ui.BirthTime) < new DateTime(2020, 1, 1) - new DateTime(2000, 1, 1))
                                            {
                                                status = 3;
                                                statusMessage = "Возраст заемщика меньше 20 лет";
                                            }
                                            else
                                            {
                                                double probability = 1.0;
                                                // проверка дополнительной информации
                                                var userInfo = db.UserInfos.FirstOrDefault(ui => ui.Id == request_ui.Id);
                                                var random = new Random();
                                                if (!userInfo.hasFamily)
                                                    probability = probability - (Convert.ToDouble(random.Next(4, 11)) / 100);
                                                if (!userInfo.hasChildren)
                                                    probability = probability - (Convert.ToDouble(random.Next(4, 11)) / 100);
                                                if (!userInfo.hasCar)
                                                    probability = probability - (Convert.ToDouble(random.Next(4, 11)) / 100);
                                                if (!userInfo.hasHigherEducation)
                                                    probability = probability - (Convert.ToDouble(random.Next(4, 11)) / 100);
                                                if (!userInfo.hasCreditInAnotherBank)
                                                    probability = probability - (Convert.ToDouble(random.Next(4, 11)) / 100);
                                                // если уже есть заверешенные кредиты
                                                if (db.Credits.Any(c => c.UserId == request_ui.Id && c.Status == 100))
                                                    probability = probability + (Convert.ToDouble(random.Next(4, 11)) / 100);
                                                // оценка вероятности
                                                if (probability < 0.70)
                                                {
                                                    status = 3;
                                                    statusMessage = "Кредит не одобрен по причинам отсутствия уверенности в заемщике";
                                                    
                                                }
                                                else
                                                {
                                                    Console.WriteLine($"Probability {probability}/70");

                                                    // подсчет кредитов
                                                    var user_credits = db.Credits.Where(c => c.UserId == request_ui.Id).ToList();
                                                    var closed = 0;
                                                    var active = 0;
                                                    var waiting = 0;
                                                    var failed = 0;
                                                    var debts = 0;

                                                    var current_common_payment = 0.0;
                                                    var potential_extra_payment = 0.0;
                                                    foreach (var cred in user_credits)
                                                    {
                                                        if (cred.isOverdue)
                                                            debts++;
                                                        if (cred.Status == 100)
                                                            closed++;
                                                        if (cred.Status == 99)
                                                        {
                                                            active++;
                                                            current_common_payment += cred.MonthlyPayment;
                                                        }
                                                        if (cred.Status == 0)
                                                        {
                                                            waiting++;
                                                            potential_extra_payment += cred.MonthlyPayment;
                                                        }
                                                        if (cred.Status > 0 && cred.Status < 99)
                                                            failed++;

                                                    }
                                                    var credits_count = user_credits.Count - failed - waiting;

                                                    // если много долгов
                                                    if (debts > active * 0.5 && active > 1)
                                                    {
                                                        status = 4;
                                                        statusMessage = "Много долгов по кредитам";
                                                    }
                                                    else
                                                    {
                                                        // если зп пользователя станет меньше чем общая сумма выплат по кредитам
                                                        if (request_oi.Salary - 5000 - request_oi.Salary * 0.13 <= current_common_payment + potential_extra_payment)
                                                        {
                                                            status = 2;
                                                            statusMessage = "Заработная плата клиента меньше суммы выплат по всем активным кредитам";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        // изменение кредита в зависимости от результата
                        credit.Status = status;
                        credit.StatusMessage = statusMessage;
                        credit.ApprovalDate = DateTime.UtcNow;

                        db.CreditProccessings.Remove(cP);
                        db.SaveChanges();
                    }
                }
            }
        }

        public void CheckDebts(int userId)
        {
            var credits = db.Credits.Where(cr => cr.UserId == userId).ToList();
            // просрочен - дата щас больше даты некст платежа
            foreach (var credit in credits)
            {
                if (DateTime.UtcNow >= credit.NextPaymentDate.AddDays(1))
                {
                    credit.isOverdue = true;
                    // credit.DebtAmount += credit.MonthlyPayment;
                }
            }
            db.SaveChanges();
        }
    }
}