using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using TenureListener.Domain.Person;
using TenureListener.Infrastructure;

namespace TenureListener.Tests.E2ETests.Fixtures
{
    public class TenureFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly IDynamoDBContext _dbContext;

        public TenureInformationDb Tenure { get; private set; }
        public List<TenureInformationDb> Tenures { get; private set; }

        public Guid TenureId { get; private set; }
        private readonly List<Action> _cleanup = new List<Action>();

        public TenureFixture(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != Tenure)
                    _dbContext.DeleteAsync<TenureInformationDb>(Tenure.Id).GetAwaiter().GetResult();

                if ((Tenures != null) && Tenures.Any())
                {
                    foreach (var t in Tenures)
                        _dbContext.DeleteAsync<TenureInformationDb>(t.Id).GetAwaiter().GetResult();
                }

                _disposed = true;
            }
        }

        private TenureInformationDb ConstructAndSaveTenure(Guid tenureId, bool nullTenuredAssetType, Guid? personId = null)
        {
            var tenure = _fixture.Build<TenureInformationDb>()
                                 .With(x => x.Id, tenureId)
                                 .With(x => x.EndOfTenureDate, DateTime.UtcNow)
                                 .With(x => x.StartOfTenureDate, DateTime.UtcNow)
                                 .With(x => x.SuccessionDate, DateTime.UtcNow)
                                 .With(x => x.PotentialEndDate, DateTime.UtcNow)
                                 .With(x => x.SubletEndDate, DateTime.UtcNow)
                                 .With(x => x.EvictionDate, DateTime.UtcNow)
                                 .With(x => x.VersionNumber, (int?) null)
                                 .Create();
            if (nullTenuredAssetType)
                tenure.TenuredAsset.Type = null;
            if (personId.HasValue)
                tenure.HouseholdMembers.Last().Id = personId.Value;

            _dbContext.SaveAsync<TenureInformationDb>(tenure).GetAwaiter().GetResult();
            return tenure;
        }

        public void GivenATenureAlreadyExists(Guid tenureId)
        {
            GivenATenureAlreadyExists(tenureId, false);
        }
        public void GivenATenureAlreadyExists(Guid tenureId, bool nullTenuredAssetType)
        {
            if (null == Tenure)
            {
                var tenure = ConstructAndSaveTenure(tenureId, nullTenuredAssetType);
                Tenure = tenure;
                TenureId = tenure.Id;
            }
        }

        public void GivenATenureDoesNotExist(Guid tenureId)
        {
            // Nothing to do here
        }

        public void GivenTenuresAlreadyExist(PersonResponseObject person, bool personInHouseholdMembers)
        {
            Tenures = new List<TenureInformationDb>();
            foreach (var personTenure in person.Tenures)
            {
                Guid? personTenureId = null;
                if (personInHouseholdMembers)
                    personTenureId = person.Id;
                var tenure = ConstructAndSaveTenure(personTenure.Id, false, personTenureId);
                Tenures.Add(tenure);
            }
        }
    }
}
