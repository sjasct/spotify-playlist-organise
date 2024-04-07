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
    
    public static async Task AddToPlaylist(this SpotifyClient client, string playlistId, IEnumerable<PlaylistTrack<FullTrack>> tracks)
    {
        var list = tracks.ToList();
        if (list.Any())
        {
            foreach (var addChunk in list.Chunk(100))
            {
                await client.Playlists.AddItems(playlistId,
                    new PlaylistAddItemsRequest(addChunk.Select(x => x.Track.Uri).ToList()));
            }
        }
    }
    
    public static async Task RemoveFromPlaylist(this SpotifyClient client, string playlistId, IEnumerable<PlaylistTrack<FullTrack>> tracks)
    {
        var list = tracks.ToList();
        if (list.Any())
        {
            foreach (var chunk in list.Chunk(100))
            {
                await client.Playlists.RemoveItems(playlistId,
                    new PlaylistRemoveItemsRequest()
                    {
                        Tracks = chunk.Select(y => new PlaylistRemoveItemsRequest.Item()
                        {
                            Uri = y.Track.Uri
                        }).ToList()
                    });
            }
        }
    }

    public static bool NotIn(this PlaylistTrack<FullTrack> track,
        List<PlaylistTrack<FullTrack>> playlist)
    {
        return playlist.All(x => x.Track.Id != track.Track.Id);
    }

    public static async Task<List<PlaylistTrack<FullTrack>>> GetPlaylistItems(this SpotifyClient client, string playlistId)
    {
        var playlist = await client.Playlists.GetItems(playlistId, new PlaylistGetItemsRequest()
        {
            Fields = { "next, items(added_on, track(id,type,uri))" }
        });
        var items = playlist != null
            ? (await client.PaginateAll(playlist)).AsFullTracks()
            : new List<PlaylistTrack<FullTrack>>();
        return items;
    }
}