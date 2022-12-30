namespace Foxtaur.LibWebClient.Models.DTOs;

/// <summary>
/// Team
/// </summary>
public class TeamDto
{
    /// <summary>
    /// Team Id
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; }

    public TeamDto(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        Id = id;
        Name = name;
    }
}