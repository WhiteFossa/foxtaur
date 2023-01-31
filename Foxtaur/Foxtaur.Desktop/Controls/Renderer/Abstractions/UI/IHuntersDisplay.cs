using System.Collections.Generic;
using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.LibRenderer.Models.UI;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;

/// <summary>
/// Like UI display, but to show hunters
/// </summary>
public interface IHuntersDisplay
{
    /// <summary>
    /// If true, then next call of DrawUi() will call Initialize() internally.
    /// Use it in case of viewport size changes
    /// </summary>
    public bool IsNeedToReinitialize { get; set; }

    /// <summary>
    /// Initialize UI
    /// </summary>
    void Initialize(GL silkGlContext, int uiWidth, int uiHeight);

    /// <summary>
    /// De-initialize UI
    /// </summary>
    void DeInitialize();

    /// <summary>
    /// Draw hunters display. Call it from OnRender()!
    /// </summary>
    void DrawUi(GL silkGlContext, int uiWidth, int uiHeight, IReadOnlyCollection<Hunter> hunters);
}