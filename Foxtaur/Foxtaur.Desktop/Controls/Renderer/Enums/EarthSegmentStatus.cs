namespace Foxtaur.Desktop.Controls.Renderer.Enums;

/// <summary>
/// Earth segment status
/// </summary>
public enum EarthSegmentStatus
{
    /// <summary>
    /// We have some buffers and meshes and need to clear in on next frame in renderer thread
    /// </summary>
    ReadyForPurge,
    
    /// <summary>
    /// Meshes and buffers are null, ready for regeneration
    /// </summary>
    ReadyForRegeneration,
    
    /// <summary>
    /// New mesh generation in progress - separate thread
    /// </summary>
    NewMeshGeneration,
    
    /// <summary>
    /// Ready for meshes swap - on next frame in renderer thread meshes will be swapped
    /// </summary>
    ReadyForMeshesSwap,
    
    /// <summary>
    /// Ready for buffers generation - on next frame in renderer thread buffers will be generated
    /// </summary>
    ReadyForBuffersGeneration,
    
    /// <summary>
    /// Segment is ready for display
    /// </summary>
    Ready
}