using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Client;

namespace Popcorn.Services.Tmdb
{
    public interface ITmdbService
    {
        Task<TMDbClient> GetClient { get; }
    }
}
