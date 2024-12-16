using System.Threading;
using System.Threading.Tasks;
using Popcorn.Models.Movie;

namespace Popcorn.Services.Movies.Trailer
{
    public interface IMovieTrailerService
    {
        Task LoadTrailerAsync(MovieJson movie, CancellationToken ct);
    }
}
