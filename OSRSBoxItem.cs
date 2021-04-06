using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OSItemIndex.Models
{
    public class OsrsBoxItem : ItemEntity
    {
        /// <summary>
        /// If the item is a duplicate.
        /// </summary>
        [Required]
        [JsonPropertyName("duplicate")]
        public bool? Duplicate { get; set; }

        /// <summary>
        /// If the item is noted.
        /// </summary>
        [Required]
        [JsonPropertyName("noted")]
        public bool? Noted { get; set; }

        /// <summary>
        /// If the item is a placeholder.
        /// </summary>
        [Required]
        [JsonPropertyName("placeholder")]
        public bool? Placeholder { get; set; }

        /// <summary>
        /// If the item is stackable (in inventory).
        /// </summary>
        [Required]
        [JsonPropertyName("stackable")]
        public bool? Stackable { get; set; }

        /// <summary>
        /// If the item is tradeable (only on GE).
        /// </summary>
        [Required]
        [JsonPropertyName("tradeable_on_ge")]
        public bool? TradeableOnGe { get; set; }

        /// <summary>
        /// The last time (UTC) the item was updated (in ISO8601 date format).
        /// </summary>
        [Required]
        [JsonPropertyName("last_updated")]
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// OSRSBox item document.
        /// </summary>
        [Required]
        [Column(TypeName = "json")]
        [JsonPropertyName("document")]
        public string Document { get; set; }
    }
}
