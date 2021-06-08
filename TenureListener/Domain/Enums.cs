using System.Text.Json.Serialization;

namespace TenureListener.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HouseholdMembersType
    {
        Person,
        Organisation
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TenuredAssetType
    {
        Dwelling,
        Garage
    }
}
