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
    public static float GetClosestLatNode(this float lat, float meshesLatGranularity)
    {
        return meshesLatGranularity * (float)Math.Floor(lat / meshesLatGranularity);
    }
    
    /// <summary>
    /// Get longitude of closest global meshes node (move eastward)
    /// </summary>
    public static float GetClosestLonNodeWest(this float lon, float meshesLonGranularity)
    {
        return meshesLonGranularity * (float)Math.Ceiling(lon / meshesLonGranularity);
    }
    
    /// <summary>
    /// Get longitude of closest global meshes node (move westward)
    /// </summary>
    public static float GetClosestLonNodeEast(this float lon, float meshesLonGranularity)
    {
        return meshesLonGranularity * (float)Math.Floor(lon / meshesLonGranularity);
    }
}