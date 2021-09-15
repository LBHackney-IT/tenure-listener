using System.Linq;
using Hackney.Shared.Tenure;
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
                Terminated = databaseEntity.Terminated,
                TenureType = databaseEntity.TenureType,
                TenuredAsset = databaseEntity.TenuredAsset,
                SuccessionDate = databaseEntity.SuccessionDate,
                AgreementType = databaseEntity.AgreementType,
                Charges = databaseEntity.Charges,
                EndOfTenureDate = databaseEntity.EndOfTenureDate,
                EvictionDate = databaseEntity.EvictionDate,
                HouseholdMembers = databaseEntity.HouseholdMembers,
                InformHousingBenefitsForChanges = databaseEntity.InformHousingBenefitsForChanges,
                IsMutualExchange = databaseEntity.IsMutualExchange,
                IsSublet = databaseEntity.IsSublet,
                IsTenanted = databaseEntity.IsTenanted,
                LegacyReferences = databaseEntity.LegacyReferences,
                Notices = databaseEntity.Notices,
                PaymentReference = databaseEntity.PaymentReference,
                PotentialEndDate = databaseEntity.PotentialEndDate,
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
                Terminated = entity.Terminated,
                TenureType = entity.TenureType,
                TenuredAsset = entity.TenuredAsset,
                SuccessionDate = entity.SuccessionDate,
                AgreementType = entity.AgreementType,
                Charges = entity.Charges,
                EndOfTenureDate = entity.EndOfTenureDate,
                EvictionDate = entity.EvictionDate,
                HouseholdMembers = entity.HouseholdMembers.ToList(),
                InformHousingBenefitsForChanges = entity.InformHousingBenefitsForChanges,
                IsMutualExchange = entity.IsMutualExchange,
                IsSublet = entity.IsSublet,
                IsTenanted = entity.IsTenanted,
                LegacyReferences = entity.LegacyReferences.ToList(),
                Notices = entity.Notices.ToList(),
                PaymentReference = entity.PaymentReference,
                PotentialEndDate = entity.PotentialEndDate,
                StartOfTenureDate = entity.StartOfTenureDate,
                SubletEndDate = entity.SubletEndDate,
                VersionNumber = entity.VersionNumber
            };
        }
    }
}
