using Foxtaur.LibWebClient.Models;

namespace Foxtaur.LibWebClient.Services.Abstract;

/// <summary>
/// High-level web client
/// </summary>
public interface IWebClient
{
    /// <summary>
    /// Get list of all distances (without including data on hunters, foxes and so on)
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyCollection<Distance>> GetDistancesWithoutIncludeAsync();
}