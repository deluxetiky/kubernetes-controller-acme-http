using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace watch.Model.Challange.Kubernetes
{
    public class ChallangeSpec
    {
        [JsonProperty("authzURL")]
        public string AuthenticationUrl { get; set; } = "";

        [JsonProperty("dnsName")]
        public string DnsName { get; set; } = "";

        [JsonProperty("key")]
        public string Key { get; set; } = "";

        [JsonProperty("token")]
        public string Token { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("wildcard")]
        public bool Wildcard { get; set; } = false;

    }
}
