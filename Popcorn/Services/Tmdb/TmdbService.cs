using System;
using System.Threading.Tasks;
using Popcorn.Helpers;
using Popcorn.Utils;
using TMDbLib.Client;

namespace Popcorn.Services.Tmdb
{
    public class TmdbService : ITmdbService
    {
        private Lazy<Task<TMDbClient>> Client { get; } = new Lazy<Task<TMDbClient>>(async () =>
        {
            var client = new TMDbClient(Constants.TmDbClientId, true);
            await client.GetConfigAsync();
            if (string.IsNullOrEmpty(client.DefaultLanguage))
                client.DefaultLanguage = "en";

            return client;
        });

        public Task<TMDbClient> GetClient => Client.Value;
    }
}