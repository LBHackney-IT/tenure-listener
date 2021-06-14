namespace TenureListener.Domain
{
    public class Charges
    {
        public float Rent { get; set; }

        public float CurrentBalance { get; set; }

        public string BillingFrequency { get; set; }

        public int PaymentReference { get; set; }

        public string RentGroupCode { get; set; }

        public string RentGroupDescription { get; set; }

        public float ServiceCharge { get; set; }

        public float OtherCharges { get; set; }

        public float CombinedServiceCharges { get; set; }

        public float CombinedRentCharges { get; set; }

        public float TenancyInsuranceCharge { get; set; }

        public float OriginalRentCharge { get; set; }

        public float OriginalServiceCharge { get; set; }
    }
}
