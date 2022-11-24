using Foxtaur.LibResources.Models;

namespace Foxtaur.LibResources.Services.Abstractions;

/// <summary>
/// Fragmented resources provider
/// </summary>
public interface IFragmentedResourcesProvider
{
    /// <summary>
    /// Get resource by geocoordinates
    /// </summary>
    FragmentedResourceBase GetResource(float lat, float lon);
}