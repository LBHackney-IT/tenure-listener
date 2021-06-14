using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TenureListener.Domain
{
    public class TenureType
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public static class TenureTypes
    {
        public static TenureType AsylumSeeker => new TenureType() { Code = "ASY", Description = "Asylum Seeker" };
        public static TenureType CommercialLet => new TenureType() { Code = "COM", Description = "Commercial Let" };
        public static TenureType TempDecant => new TenureType() { Code = "DEC", Description = "Temp Decant" };
        public static TenureType Freehold => new TenureType() { Code = "FRE", Description = "Freehold" };
        public static TenureType FreeholdServ => new TenureType() { Code = "FRS", Description = "Freehold (Serv)" };
        public static TenureType Introductory => new TenureType() { Code = "INT", Description = "Introductory" };
        public static TenureType LeaseholdRTB => new TenureType() { Code = "LEA", Description = "Leasehold (RTB)" };
        public static TenureType LeaseholdStair => new TenureType() { Code = "LHS", Description = "Lse 100% Stair" };
        public static TenureType LicenseTempAc => new TenureType() { Code = "LTA", Description = "License Temp Ac" };
        public static TenureType MesneProfitAc => new TenureType() { Code = "MPA", Description = "Mesne Profit Ac" };
        public static TenureType NonSecure => new TenureType() { Code = "NON", Description = "Non-Secure" };
        public static TenureType PrivateGarage => new TenureType() { Code = "PVG", Description = "Private Garage" };
        public static TenureType RegisteredSocialLandlord => new TenureType() { Code = "RSL", Description = "Registered Social Landlord" };
        public static TenureType RenttoMortgage => new TenureType() { Code = "RTM", Description = "RenttoMortgage" };
        public static TenureType Secure => new TenureType() { Code = "SEC", Description = "Secure" };
        public static TenureType SharedOwners => new TenureType() { Code = "SHO", Description = "Shared Owners" };
        public static TenureType ShortLifeLse => new TenureType() { Code = "SLL", Description = "Short Life Lse" };
        public static TenureType PrivateSaleLH => new TenureType() { Code = "SPS", Description = "Private Sale LH" };
        public static TenureType SharedEquity => new TenureType() { Code = "SSE", Description = "Shared Equity" };
        public static TenureType TenantAccFlat => new TenureType() { Code = "TAF", Description = "Tenant Acc Flat" };
        public static TenureType TempBAndB => new TenureType() { Code = "TBB", Description = "Temp B&B" };
        public static TenureType TenantGarage => new TenureType() { Code = "TGA", Description = "Tenant Garage" };
        public static TenureType TempHostelLse => new TenureType() { Code = "THL", Description = "Temp Hostel Lse" };
        public static TenureType TempHostel => new TenureType() { Code = "THO", Description = "Temp Hostel" };
        public static TenureType TempAnnex => new TenureType() { Code = "TLA", Description = "Temp Annex" };
        public static TenureType TempPrivateLt => new TenureType() { Code = "TPL", Description = "Temp Private Lt" };
        public static TenureType TempTraveller => new TenureType() { Code = "TRA", Description = "Temp Traveller" };

        public static List<string> FreeholderCodes => new List<string>
        {
            Freehold.Code,
            FreeholdServ.Code
        };
        public static List<string> LeaseholderCodes => new List<string>
        {
            LeaseholdRTB.Code,
            RegisteredSocialLandlord.Code
        };

        public static PersonTenureType GetPersonTenureType(this TenureType tenureType, bool isResponsible)
        {
            if (!isResponsible)
            {
                if (tenureType.Code == TenantGarage.Code) return PersonTenureType.Occupant;
                else return PersonTenureType.HouseholdMember;
            }
            else
            {
                if (FreeholderCodes.Contains(tenureType.Code)) return PersonTenureType.Freeholder;
                if (LeaseholderCodes.Contains(tenureType.Code)) return PersonTenureType.Leaseholder;
                else return PersonTenureType.Tenant;
            }
        }
    }
}
