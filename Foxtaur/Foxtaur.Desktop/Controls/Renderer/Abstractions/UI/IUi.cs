using Foxtaur.LibRenderer.Models.UI;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;

/// <summary>
/// Iterface for user interface
/// </summary>
public interface IUi
{
    /// <summary>
    /// Initialize UI
    /// </summary>
    void Initialize(GL silkGlContext, int uiWidth, int uiHeight, UiData data);

    /// <summary>
    /// De-initialize UI
    /// </summary>
    void DeInitialize();
    
    /// <summary>
    /// Generate UI
    /// </summary>
    void GenerateUi(GL silkGlContext, int uiWidth, int uiHeight, UiData data);

    /// <summary>
    /// Draw GUI. Call it from OnRender()!
    /// </summary>
    void DrawUi(GL silkGlContext);
}