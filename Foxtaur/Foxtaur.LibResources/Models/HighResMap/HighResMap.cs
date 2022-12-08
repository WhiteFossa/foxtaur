namespace Foxtaur.LibResources.Models.HighResMap;

/// <summary>
/// Small, high-resolution map
/// </summary>
public class HighResMap
{
    /// <summary>
    /// Map ID
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Furry-readable map name
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Actual map fragment
    /// </summary>
    public HighResMapFragment Fragment { get; private set; }

    public HighResMap(Guid id, string name, HighResMapFragment fragment)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException(nameof(name));
        }

        Id = id;
        Name = name;
        Fragment = fragment ?? throw new ArgumentNullException(nameof(fragment));
    }
}