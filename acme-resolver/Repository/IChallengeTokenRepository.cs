using System.Threading.Tasks;
using watch.Model.Challange.Kubernetes;

namespace acme_resolver.Repository
{
    public interface IChallengeTokenRepository
    {
        Task RegisterTokenAsync(ChallangeSpec entity);
        Task UpdateTokenAsync(ChallangeSpec entity);
        Task<ChallangeSpec> GetKeyByToken(string token);
        Task DeleteTokenAsyc(string token);
    }
}
