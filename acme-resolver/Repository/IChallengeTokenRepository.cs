using System;
using System.Threading.Tasks;
using watch.Model.Challange;

namespace acme_resolver.Services
{
    public interface IChallengeTokenRepository
    {
        Task RegisterTokenAsync(TokenEntity entity);
        Task<TokenEntity> GetKeyByToken(string token);
    }
}
