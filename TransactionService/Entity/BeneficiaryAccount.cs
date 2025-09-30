using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransactionService.Entity
{
    public class BeneficiaryAccount
    {
        public string AccountNumber { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string BankName { get; private set; } = string.Empty;
    }
}