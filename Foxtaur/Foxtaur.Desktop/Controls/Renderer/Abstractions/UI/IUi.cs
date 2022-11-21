using Foxtaur.LibRenderer.Models.UI;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;

/// <summary>
/// Iterface for user interface
/// </summary>
public interface IUi
{
    /// <summary>
    /// Generate UI
    /// </summary>
    Texture GenerateUi(GL silkGlContext, int uiWidth, int uiHeight, UiData data);
}