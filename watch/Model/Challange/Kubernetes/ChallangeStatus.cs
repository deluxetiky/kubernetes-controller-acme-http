using System;
using Newtonsoft.Json;

namespace watch.Model.Challange.Kubernetes
{
    public class ChallangeStatus
    {
        [JsonProperty("presented")]
        public bool Presented { get; set; } = false;

        [JsonProperty("processing")]
        public bool Processing { get; set; } = false;

        [JsonProperty("Reason")]
        public string Reason { get; set; } = "";
    }
}
