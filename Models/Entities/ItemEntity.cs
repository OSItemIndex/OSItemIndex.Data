using System.Text.Json.Serialization;

namespace OSItemIndex.Data
{
    public class ItemEntity : Entity
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
