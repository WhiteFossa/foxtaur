namespace Foxtaur.LibWebClient.Models;

/// <summary>
/// Distance
/// </summary>
public class Distance
{
    /// <summary>
    /// Distance ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Associated map
    /// </summary>
    public Map Map { get; }

    /// <summary>
    /// Is someone running here?
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Start location
    /// </summary>
    public Location StartLocation { get; }

    /// <summary>
    /// Finish location
    /// </summary>
    public Location FinishLocation { get; }

    /// <summary>
    /// Foxes on distance
    /// </summary>
    public IReadOnlyCollection<Fox> Foxes { get; }

    /// <summary>
    /// Hunters on distance
    /// </summary>
    public IReadOnlyCollection<Hunter> Hunters { get; }

    public Distance(
        Guid id,
        string name,
        Map map,
        bool isActive,
        Location startLocation,
        Location finishLocation,
        IReadOnlyCollection<Fox> foxes,
        IReadOnlyCollection<Hunter> hunters)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        Id = id;
        Name = name;
        Map = map ?? throw new ArgumentNullException(nameof(map));
        IsActive = isActive;
        StartLocation = startLocation ?? throw new ArgumentNullException(nameof(startLocation));
        FinishLocation = finishLocation ?? throw new ArgumentNullException(nameof(finishLocation));
        Foxes = foxes ?? throw new ArgumentNullException(nameof(foxes));
        Hunters = hunters ?? throw new ArgumentNullException(nameof(hunters));
    }
}