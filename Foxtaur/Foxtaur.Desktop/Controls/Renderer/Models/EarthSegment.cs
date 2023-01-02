using System;
using System.Threading;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Enums;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibRenderer.Constants;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Models;

/// <summary>
/// Earth segment: geodata + mesh
/// </summary>
public class EarthSegment
{
    /// <summary>
    /// Earth segment status
    /// </summary>
    public EarthSegmentStatus Status { get; private set; }

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

    private IEarthGenerator _earthGenerator = Program.Di.GetService<IEarthGenerator>();
    
    private Logger _logger = LogManager.GetCurrentClassLogger();

    protected static Semaphore RegenerationLimiter = new Semaphore(RendererConstants.SegmentsRegenerationThreads, RendererConstants.SegmentsRegenerationThreads);

    public EarthSegment(GeoSegment geoSegment)
    {
        GeoSegment = geoSegment ?? throw new ArgumentNullException(nameof(geoSegment));
        
        Status = EarthSegmentStatus.ReadyForRegeneration;
    }

    /// <summary>
    /// Purge segment, call only from rendering code
    /// </summary>
    public void Purge()
    {
        if (Status != EarthSegmentStatus.ReadyForPurge)
        {
            throw new InvalidOperationException($"Trying to purge segment, but it is in { Status } state!");
        }
        
        if (NewMesh != null)
        {
            NewMesh.Dispose();
            NewMesh = null;
        }

        if (Mesh != null)
        {
            Mesh.Dispose();
            Mesh = null;
        }

        SetStatus(EarthSegmentStatus.ReadyForRegeneration);
    }
    
    /// <summary>
    /// Swap meshes, call only from rendering code
    /// </summary>
    public void SwapMeshes()
    {
        if (Status != EarthSegmentStatus.ReadyForMeshesSwap)
        {
            throw new InvalidOperationException($"Trying to swap meshes, but segment is in { Status } state!");
        }
        
        if (Mesh != null)
        {
            Mesh.Dispose();
        }
        
        Mesh = NewMesh ?? throw new ArgumentNullException(nameof(NewMesh));
        NewMesh = null;

        SetStatus(EarthSegmentStatus.ReadyForBuffersGeneration);
    }

    /// <summary>
    /// Generate buffers, call only from rendering code
    /// </summary>
    public void GenerateBuffers(GL silkGl)
    {
        if (Status != EarthSegmentStatus.ReadyForBuffersGeneration)
        {
            throw new InvalidOperationException($"Trying to generate buffers, but segment is in { Status } state!");
        }
        
        Mesh.GenerateBuffers(silkGl);
        
        SetStatus(EarthSegmentStatus.Ready);
    }

    /// <summary>
    /// Regenerate segment's mesh (designed to be called in separate thread)
    /// </summary>
    public void RegenerateMesh(ref int threadsCount)
    {
        RegenerationLimiter.WaitOne();

        try
        {
            if (Status != EarthSegmentStatus.ReadyForRegeneration)
            {
                throw new InvalidOperationException($"Attempt to regenerate segment, while it is in { Status } state!");
            }

            Status = EarthSegmentStatus.NewMeshGeneration;

            // Actual regeneration
            _earthGenerator.GenerateMeshForSegment(this);

            Status = EarthSegmentStatus.ReadyForMeshesSwap;
        }
        finally
        {
            RegenerationLimiter.Release();

            threadsCount--;
        }
    }

    /// <summary>
    /// Set new status
    /// </summary>
    public void SetStatus(EarthSegmentStatus status)
    {
        RegenerationLimiter.WaitOne();

        try
        {
            Status = status;
        }
        finally
        {
            RegenerationLimiter.Release();
        }
    }
}