namespace Foxtaur.LibWebClient.Models.DTOs;

/// <summary>
/// Fox
/// </summary>
public class FoxDto
{
    /// <summary>
    /// Fox ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Fox name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Fox frequency in Hz
    /// </summary>
    public double Frequency { get; }

    /// <summary>
    /// Fox code
    /// </summary>
    public string Code { get; }

    public FoxDto(
        Guid id,
        string name,
        double frequency,
        string code)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(nameof(code));
        }

        Id = id;
        Name = name;
        Frequency = frequency;
        Code = code;
    }
}