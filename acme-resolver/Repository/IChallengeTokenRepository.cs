using System;
using System.Threading.Tasks;
using watch.Model.Challange;
using watch.Model.Challange.Kubernetes;

namespace acme_resolver.Services
{
    public interface IChallengeTokenRepository
    {
        Task RegisterTokenAsync(ChallangeSpec entity);
        Task UpdateTokenAsync(ChallangeSpec entity);
        Task<ChallangeSpec> GetKeyByToken(string token);
        Task DeleteTokenAsyc(string token);
    }
}
