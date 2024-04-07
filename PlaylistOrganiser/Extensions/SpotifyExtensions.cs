using SpotifyAPI.Web;

namespace PlaylistOrganiser.Extensions;

public static class SpotifyExtensions
{
    public static List<PlaylistTrack<FullTrack>> AsFullTracks(this IList<PlaylistTrack<IPlayableItem>> items)
    {
        return items.Where(it => it.Track is FullTrack).Select(x => new PlaylistTrack<FullTrack>
        {
            Track = (x.Track as FullTrack)!,
            AddedAt = x.AddedAt,
            AddedBy = x.AddedBy,
            IsLocal = x.IsLocal
        }).ToList();
    }
}