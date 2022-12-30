namespace Foxtaur.LibWebClient.Models;

/// <summary>
/// Hunter
/// </summary>
public class Hunter
{
    /// <summary>
    /// Hunter Id
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// True if hunter is on distance now
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// Team, may be null if hunter is teamless
    /// </summary>
    public Team Team { get; }

    /// <summary>
    /// Hunter's latitude
    /// </summary>
    public double Lat { get; }

    /// <summary>
    /// Hunter's longitude
    /// </summary>
    public double Lon { get; }

    public Hunter(
        Guid id,
        string name,
        bool isRunning,
        Team team,
        double lat,
        double lon)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        Id = id;
        Name = name;
        IsRunning = isRunning;
        Team = team;
        Lat = lat;
        Lon = lon;
    }
}