using Popcorn.Models.Shows;
using System.Threading;
using System.Threading.Tasks;

namespace Popcorn.Services.Shows.Trailer
{
    public interface IShowTrailerService
    {
        Task LoadTrailerAsync(ShowJson show, CancellationToken ct);
    }
}
