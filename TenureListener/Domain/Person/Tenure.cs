using System;

namespace TenureListener.Domain.Person
{
    public class Tenure
    {
        public string AssetFullAddress { get; set; }

        public string AssetId { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Uprn { get; set; }

        public bool IsActive { get; set; }
    }
}
