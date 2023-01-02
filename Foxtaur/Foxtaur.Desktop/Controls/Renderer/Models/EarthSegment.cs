using System;
using System.Threading;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibRenderer.Constants;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Foxtaur.Desktop.Controls.Renderer.Models;

/// <summary>
/// Earth segment: geodata + mesh
/// </summary>
public class EarthSegment
{
    /// <summary>
    /// Coordinates of segment
    /// </summary>
    public GeoSegment GeoSegment { get; private set; }

    /// <summary>
    /// Mesh for given segment
    /// </summary>
    public Mesh Mesh { get; private set; }

    /// <summary>
    /// New mesh, just a mesh will be replaced with it on next frame if this one is not null
    /// </summary>
    public Mesh NewMesh { get; set; }

    /// <summary>
    /// If true, then mesh is successfully regenerated and it's time to regenerate buffer
    /// </summary>
    public bool IsMeshReady { get; set; }

    /// <summary>
    /// Mesh regeneration process initiated, but not completed yet
    /// </summary>
    public bool IsMeshRegenerating { get; set; }

    /// <summary>
    /// If true, then buffer (and segment as a whole) is ready
    /// </summary>
    public bool IsBufferReady { get; set; }
    
    private IEarthGenerator _earthGenerator = Program.Di.GetService<IEarthGenerator>();
    
    private Logger _logger = LogManager.GetCurrentClassLogger();

    protected static Semaphore RegenerationLimiter = new Semaphore(RendererConstants.SegmentsRegenerationThreads, RendererConstants.SegmentsRegenerationThreads);

    public EarthSegment(GeoSegment geoSegment)
    {
        GeoSegment = geoSegment ?? throw new ArgumentNullException(nameof(geoSegment));

        MarkToRegeneration();
    }

    /// <summary>
    /// Mark segment as needing regeneration
    /// </summary>
    public void MarkToRegeneration()
    {
        IsMeshReady = false;
        IsBufferReady = false;
    }

    /// <summary>
    /// Update mesh
    /// </summary>
    public void UpdateMesh()
    {
        if (Mesh != null)
        {
            Mesh.Dispose();
        }
        
        Mesh = NewMesh ?? throw new ArgumentNullException(nameof(NewMesh));
        NewMesh = null;
    }

    /// <summary>
    /// Regenerate segment's mesh (designed to be called in separate thread)
    /// </summary>
    public void RegenerateMesh(ref int threadsCount)
    {
        RegenerationLimiter.WaitOne();

        try
        {
            if (IsMeshReady)
            {
                return;
            }

            if (IsMeshRegenerating)
            {
                return;
            }

            IsMeshRegenerating = true;

            // Actual regeneration
            _earthGenerator.GenerateMeshForSegment(this);

            IsMeshRegenerating = false;
            IsMeshReady = true;
        }
        finally
        {
            RegenerationLimiter.Release();

            threadsCount--;
        }
    }
}