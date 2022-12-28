using System.Text.Json.Serialization;
using Avalonia.Input;

namespace Foxtaur.LibSettings.Services.Implementations;

/// <summary>
/// Class with settings to store in file (see settings descriptions in ISettingsService)
/// </summary>
public class SerializableSettings
{
    [JsonPropertyName("DemScale")]
    public double DemScale { get; set; }
    
    [JsonPropertyName("SurfaceRunSpeed")]
    public double SurfaceRunSpeed { get; set; }
    
    [JsonPropertyName("SurfaceRunTurnSpeed")]
    public double SurfaceRunTurnSpeed { get; set; }
    
    [JsonPropertyName("SurfaceRunForwardButton")]
    public Key SurfaceRunForwardButton { get; set; }
    
    [JsonPropertyName("SurfaceRunBackwardButton")]
    public Key SurfaceRunBackwardButton { get; set; }
    
    [JsonPropertyName("SurfaceRunTurnLeftButton")]
    public Key SurfaceRunTurnLeftButton { get; set; }
    
    [JsonPropertyName("SurfaceRunTurnRightButton")]
    public Key SurfaceRunTurnRightButton { get; set; }
}