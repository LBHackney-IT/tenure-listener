using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using System;
using TenureListener.Infrastructure;

namespace TenureListener.Tests.E2ETests.Fixtures
{
    public class TenureFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly IDynamoDBContext _dbContext;

        public TenureInformationDb Tenure { get; private set; }

        public Guid TenureId { get; private set; }

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

                _disposed = true;
            }
        }

        public void GivenATenureAlreadyExists(Guid tenureId)
        {
            if (null == Tenure)
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
                _dbContext.SaveAsync<TenureInformationDb>(tenure).GetAwaiter().GetResult();
                Tenure = tenure;
                TenureId = tenure.Id;
            }
        }

        public void GivenATenureDoesNotExist()
        {
            TenureId = Guid.NewGuid();
        }
    }
}
