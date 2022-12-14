using Foxtaur.LibRenderer.Models.UI;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;

/// <summary>
/// Iterface for user interface
/// </summary>
public interface IUi
{
    /// <summary>
    /// If true, then next call of DrawUi() will call Initialize() internally.
    /// Use it in case of viewport size changes
    /// </summary>
    public bool IsNeedToReinitialize { get; set; }

    /// <summary>
    /// Initialize UI
    /// </summary>
    void Initialize(GL silkGlContext, int uiWidth, int uiHeight, UiData data);

    /// <summary>
    /// De-initialize UI
    /// </summary>
    void DeInitialize();

    /// <summary>
    /// Draw GUI. Call it from OnRender()!
    /// </summary>
    void DrawUi(GL silkGlContext, int uiWidth, int uiHeight, UiData uiData);
}