using System;
using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace watch.Model.Challange.Kubernetes
{
    [KubernetesEntity(Group = "apiextensions.k8s.io", Kind = "CustomResourceDefinition", ApiVersion = "acme.cert-manager.io/v1alpha2", PluralName = "challenges")]
    public class ChallangeResource : V1CustomResourceDefinition, IKubernetesObject<V1ObjectMeta>, IKubernetesObject, IMetadata<V1ObjectMeta>, ISpec<ChallangeSpec>, IValidate
    {
        public ChallangeResource() : base()
        {
            ApiVersion = this.GetKubernetesTypeMetadata().ApiVersion;
        }
        public ChallangeResource(V1CustomResourceDefinitionSpec spec, string apiVersion = null, string kind = null, V1ObjectMeta metadata = null, V1CustomResourceDefinitionStatus status = null) : base(spec, apiVersion, kind, metadata, status)
        {

        }
        [JsonProperty("spec")]
        public new ChallangeSpec Spec { get; set; } = new ChallangeSpec();

        [JsonProperty("status")]
        public new ChallangeStatus Status { get; set; } = new ChallangeStatus();

    }
}
