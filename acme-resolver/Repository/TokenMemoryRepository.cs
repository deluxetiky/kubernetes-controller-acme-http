using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using acme_resolver.Services;
using Serilog;
using watch.Model.Challange;
using watch.Model.Challange.Kubernetes;

namespace acme_resolver.Repository
{
    public class TokenMemoryRepository : IChallengeTokenRepository
    {
        private List<ChallangeSpec> tokens = new List<ChallangeSpec>();
        private ILogger _logger;

        public TokenMemoryRepository()
        {
            _logger = Log.ForContext<TokenMemoryRepository>();
        }

        public Task DeleteTokenAsyc(string token)
        {
            tokens = tokens.Where(a=>a.Token!=token).ToList();
            return Task.CompletedTask;
        }

        public Task<ChallangeSpec> GetKeyByToken(string token)
        {
            return Task.FromResult(tokens.FirstOrDefault(a=> a.Token==token));
        }      

        public Task RegisterTokenAsync(ChallangeSpec entity)
        {
            _logger.Debug("Token {@token} is registering to repository",entity);
            tokens.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateTokenAsync(ChallangeSpec entity)
        {
            tokens = tokens.Where(a=>a.Token!=entity.Token).ToList();
            tokens.Add(entity);
            return Task.CompletedTask;
        }
    }
}
