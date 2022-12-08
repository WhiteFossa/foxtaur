using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.LibResources.Models.HighResMap;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;

/// <summary>
/// Generator for map segments
/// </summary>
public interface IMapSegmentGenerator
{
    /// <summary>
    /// Generate map segment by map
    /// </summary>
    MapSegment Generate(HighResMap map);
}