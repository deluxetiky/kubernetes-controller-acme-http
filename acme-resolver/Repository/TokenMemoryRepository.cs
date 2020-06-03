using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using acme_resolver.Services;
using Serilog;
using watch.Model.Challange;

namespace acme_resolver.Repository
{
    public class TokenMemoryRepository : IChallengeTokenRepository
    {
        private List<TokenEntity> tokens = new List<TokenEntity>();
        private ILogger _logger;

        public TokenMemoryRepository()
        {
            _logger = Log.ForContext<TokenMemoryRepository>();
        }

        public Task<TokenEntity> GetKeyByToken(string token)
        {
            return Task.FromResult(tokens.FirstOrDefault(a=> a.Token==token));
        }      

        public Task RegisterTokenAsync(TokenEntity entity)
        {
            _logger.Debug("Token {@token} is registering to repository",entity);
            tokens.Add(entity);
            return Task.CompletedTask;
        }
    }
}
