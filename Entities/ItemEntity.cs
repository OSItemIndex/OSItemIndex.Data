﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OsItemIndex.Data
{
    public class ItemEntity
    {
        /// <summary>
        /// Unique OSRS item ID number.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        /// <summary>
        /// The name of the item.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
