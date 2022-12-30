namespace Foxtaur.LibWebClient.Models.DTOs;

/// <summary>
/// Hunter
/// </summary>
public class HunterDto
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
    /// Team ID, may be null if hunter is teamless
    /// </summary>
    public Guid? TeamId { get; }

    /// <summary>
    /// Hunter's latitude
    /// </summary>
    public double Lat { get; }

    /// <summary>
    /// Hunter's longitude
    /// </summary>
    public double Lon { get; }

    public HunterDto(
        Guid id,
        string name,
        bool isRunning,
        Guid? teamId,
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
        TeamId = teamId;
        Lat = lat;
        Lon = lon;
    }
}