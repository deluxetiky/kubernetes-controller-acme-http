using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace watch.Model
{
    public class ChallangeSpec
    {
        [JsonProperty("authzURL")]
        public String AuthenticationUrl { get; set; } = "";

        [JsonProperty("dnsName")]
        public String DnsName { get; set; } = "";

        [JsonProperty("issuerRef")]
        public JToken IssuerRef { get; set; } = new JObject();

        [JsonProperty("key")]
        public String Key { get; set; } = "";

        [JsonProperty("solver")]
        public JToken Solver { get; set; } = new JObject();

        [JsonProperty("token")]
        public String Token { get; set; } = "";

        [JsonProperty("type")]
        public String Type { get; set; } = "";

        [JsonProperty("wildcard")]
        public bool Wildcard { get; set; } = false;

    }
}
