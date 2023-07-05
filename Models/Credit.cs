namespace vkr_bank.Models
{
    public class Credit
    {
        // на клиенте
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CreditType { get; set; }
        public double CreditAmount { get; set; }
        public int CreditTerm { get; set; }
        public double MonthlyPayment { get; set; }
        // на сервере
        public int Status { get; set; }
        public string StatusMessage { get; set; }
        public DateTime ApplicationDate { get; set; } // дата подачи заявки
        public DateTime ApprovalDate { get; set; } // дата одобрения заявки
        public bool isOverdue { get; set; } // просрочен или нет

        public double DebtAmount { get; set; } // сумма долга
        public DateTime NextPaymentDate { get; set; } // дата следующего платежа
        // для дифференциального платежа
        public bool isDifferentiated { get; set; } // на клиенте
        public double CreditBalance { get; set; } // остаток кредита
        public int NumberOfPayments { get; set; } // количество оплаченных месяцев

    }
}
