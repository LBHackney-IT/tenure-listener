using System;
using System.Collections.Generic;

namespace TenureListener.Domain.Account
{
    public class AccountTenure
    {
        public Guid TenureId { get; set; }
        public string TenureType { get; set; }
        public List<AccountTenant> PrimaryTenants { get; set; }
        public string FullAddress { get; set; }
    }
}
