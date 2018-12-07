using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM
{
    public class BankAccount
    {
        public float ActualBalance { get; set; } = 0;
        public int CreditLine { get; set; } = 0;
        public int DailyMoneyOrder { get; set; } = 0;
        public float UnpaidInterest { get; set; } = 0;
        internal int AvailableMoney {
            get
            {
                return (int) Math.Floor(Math.Max(ActualBalance + (ATMMod.config.Credit ? CreditLine : 0), 0));
            }
        }
        internal int Balance
        {
            get
            {
                return (int)Math.Floor(ActualBalance);
            }
        }
    }
}
