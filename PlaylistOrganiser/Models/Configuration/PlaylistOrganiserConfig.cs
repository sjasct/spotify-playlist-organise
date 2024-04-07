namespace PlaylistOrganiser.Models.Configuration;

public class PlaylistOrganiserConfig
{
    public required List<string> Origins { get; set; }
    public required string Combined { get; set; }
    public required string Fresh { get; set; }
    public required string Archive { get; set; }
    public int FreshLimit { get; set; } = 50;
}