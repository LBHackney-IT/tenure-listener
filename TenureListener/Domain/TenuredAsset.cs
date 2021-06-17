using System;

namespace TenureListener.Domain
{
    public class TenuredAsset
    {
        public Guid Id { get; set; }

        public TenuredAssetType? Type { get; set; }

        public string FullAddress { get; set; }

        public string Uprn { get; set; }
    }
}
