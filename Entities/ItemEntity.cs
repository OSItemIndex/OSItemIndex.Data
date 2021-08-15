using System.Text.Json.Serialization;

namespace OSItemIndex.Data
{
    public class ItemEntity : IEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// The name of the item.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
