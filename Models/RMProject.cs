using System;
using Newtonsoft.Json;

namespace OSItemIndex.Observer.Models
{
    /// <summary>
    /// https://release-monitoring.org/api/{projectid} response model.
    /// </summary>
    public class RMProject
    {
        [JsonProperty("id", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("updated_on", Required = Required.Always)]
        public DateTime UpdatedOn { get; set; }

        [JsonProperty("version", Required = Required.Always)]
        public string Version { get; set; }
    }
}
