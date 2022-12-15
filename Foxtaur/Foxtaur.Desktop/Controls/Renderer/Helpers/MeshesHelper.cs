using System;

namespace Foxtaur.Desktop.Controls.Renderer.Helpers;

/// <summary>
/// Useful stuff to work with meshes
/// </summary>
public static class MeshesHelper
{
    /// <summary>
    /// Get latitude of closest global meshes node
    /// </summary>
    public static double GetClosestLatNode(this double lat, double meshesLatGranularity)
    {
        return meshesLatGranularity * Math.Floor(lat / meshesLatGranularity);
    }
    
    /// <summary>
    /// Get longitude of closest global meshes node (move eastward)
    /// </summary>
    public static double GetClosestLonNodeWest(this double lon, double meshesLonGranularity)
    {
        return meshesLonGranularity * Math.Ceiling(lon / meshesLonGranularity);
    }
    
    /// <summary>
    /// Get longitude of closest global meshes node (move westward)
    /// </summary>
    public static double GetClosestLonNodeEast(this double lon, double meshesLonGranularity)
    {
        return meshesLonGranularity * Math.Floor(lon / meshesLonGranularity);
    }
}