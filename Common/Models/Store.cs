using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Store : IEquatable<Store>
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Store);
        }

        public bool Equals(Store other)
        {
            return !(other is null) &&
                   Id == other.Id &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }

        public static bool operator ==(Store left, Store right)
        {
            return EqualityComparer<Store>.Default.Equals(left, right);
        }

        public static bool operator !=(Store left, Store right)
        {
            return !(left == right);
        }
    }
}
