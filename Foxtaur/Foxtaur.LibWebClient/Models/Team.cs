namespace Foxtaur.LibWebClient.Models;

/// <summary>
/// Team
/// </summary>
public class Team
{
    /// <summary>
    /// Team Id
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; }

    public Team(
        Guid id,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        Id = id;
        Name = name;
    }
}