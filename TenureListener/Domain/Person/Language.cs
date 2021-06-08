using System.Text.Json.Serialization;

namespace TenureListener.Domain.Person
{
    public class Language
    {
        [JsonPropertyName("language")]
        public string Name { get; set; }
        public bool IsPrimary { get; set; }
    }
}
