namespace Popcorn.Models.Torrent
{
    public interface ITorrent
    {
        int? Seeds { get; set; }

        int? Peers { get; set; }

        string Size { get; set; }

        string Url { get; set; }

        string Quality { get; set; }
    }
}
