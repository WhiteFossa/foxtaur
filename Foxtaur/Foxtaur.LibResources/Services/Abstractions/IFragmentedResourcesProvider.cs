using Foxtaur.LibResources.Enums;
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
    FragmentedResourceBase GetResource(double lat, double lon, ZoomLevel zoomLevel);
}