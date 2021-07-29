using System.Linq;
using TenureListener.Domain;
using TenureListener.Infrastructure;

namespace TenureListener.Factories
{
    public static class EntityFactory
    {
        public static TenureInformation ToDomain(this TenureInformationDb databaseEntity)
        {
            return new TenureInformation
            {
                Id = databaseEntity.Id,
                AccountType = databaseEntity.AccountType,
                Terminated = databaseEntity.Terminated,
                TenureType = databaseEntity.TenureType,
                TenuredAsset = databaseEntity.TenuredAsset,
                SuccessionDate = databaseEntity.SuccessionDate,
                SubsidiaryAccountsReferences = databaseEntity.SubsidiaryAccountsReferences,
                AgreementType = databaseEntity.AgreementType,
                Charges = databaseEntity.Charges,
                EndOfTenureDate = databaseEntity.EndOfTenureDate,
                EvictionDate = databaseEntity.EvictionDate,
                HouseholdMembers = databaseEntity.HouseholdMembers,
                InformHousingBenefitsForChanges = databaseEntity.InformHousingBenefitsForChanges,
                IsActive = databaseEntity.IsActive,
                IsMutualExchange = databaseEntity.IsMutualExchange,
                IsSublet = databaseEntity.IsSublet,
                IsTenanted = databaseEntity.IsTenanted,
                LegacyReferences = databaseEntity.LegacyReferences,
                MasterAccountTenureReference = databaseEntity.MasterAccountTenureReference,
                Notices = databaseEntity.Notices,
                PaymentReference = databaseEntity.PaymentReference,
                PotentialEndDate = databaseEntity.PotentialEndDate,
                RentCostCentre = databaseEntity.RentCostCentre,
                StartOfTenureDate = databaseEntity.StartOfTenureDate,
                SubletEndDate = databaseEntity.SubletEndDate,
                VersionNumber = databaseEntity.VersionNumber
            };
        }

        public static TenureInformationDb ToDatabase(this TenureInformation entity)
        {

            return new TenureInformationDb
            {
                Id = entity.Id,
                AccountType = entity.AccountType,
                Terminated = entity.Terminated,
                TenureType = entity.TenureType,
                TenuredAsset = entity.TenuredAsset,
                SuccessionDate = entity.SuccessionDate,
                SubsidiaryAccountsReferences = entity.SubsidiaryAccountsReferences.ToList(),
                AgreementType = entity.AgreementType,
                Charges = entity.Charges,
                EndOfTenureDate = entity.EndOfTenureDate,
                EvictionDate = entity.EvictionDate,
                HouseholdMembers = entity.HouseholdMembers.ToList(),
                InformHousingBenefitsForChanges = entity.InformHousingBenefitsForChanges,
                IsActive = entity.IsActive,
                IsMutualExchange = entity.IsMutualExchange,
                IsSublet = entity.IsSublet,
                IsTenanted = entity.IsTenanted,
                LegacyReferences = entity.LegacyReferences.ToList(),
                MasterAccountTenureReference = entity.MasterAccountTenureReference,
                Notices = entity.Notices.ToList(),
                PaymentReference = entity.PaymentReference,
                PotentialEndDate = entity.PotentialEndDate,
                RentCostCentre = entity.RentCostCentre,
                StartOfTenureDate = entity.StartOfTenureDate,
                SubletEndDate = entity.SubletEndDate,
                VersionNumber = entity.VersionNumber
            };
        }
    }
}
