using Avalonia.Input;

namespace Foxtaur.LibSettings.Services.Abstractions;

/// <summary>
/// Settings service (here we store all settings)
/// </summary>
public interface ISettingsService
{
    #region DEM scale
    
    public class OnDemScaleChangedArgs
    {
        /// <summary>
        /// New DEM scale
        /// </summary>
        public double SurfaceRunSpeed { get; }

        public OnDemScaleChangedArgs(double surfaceRunSpeed)
        {
            SurfaceRunSpeed = surfaceRunSpeed;
        }
    }
    
    /// <summary>
    /// Called when DEM scale is changed
    /// </summary>
    delegate void OnDemScaleChangedHandler(object sender, OnDemScaleChangedArgs args);

    /// <summary>
    /// Event for DEM scale change
    /// </summary>
    event OnDemScaleChangedHandler OnDemScaleChanged;
    
    /// <summary>
    /// Get current DEM scale
    /// </summary>
    double GetDemScale();

    /// <summary>
    /// Set DEM scale
    /// </summary>
    void SetDemScale(double demScale);

    #endregion
    
    #region Surface run speed

    public class OnSurfaceRunSpeedChangedArgs
    {
        /// <summary>
        /// New surface run speed
        /// </summary>
        public double SurfaceRunSpeed { get; }

        public OnSurfaceRunSpeedChangedArgs(double surfaceRunSpeed)
        {
            SurfaceRunSpeed = surfaceRunSpeed;
        }
    }
    
    /// <summary>
    /// Called when surface run speed is changed
    /// </summary>
    delegate void OnSurfaceRunSpeedChangedHandler(object sender, OnSurfaceRunSpeedChangedArgs args);

    /// <summary>
    /// Event for surface run speed change
    /// </summary>
    event OnSurfaceRunSpeedChangedHandler OnSurfaceRunSpeedChanged;
    
    /// <summary>
    /// Get current surface run speed
    /// </summary>
    double GetSurfaceRunSpeed();

    /// <summary>
    /// Set surface run speed
    /// </summary>
    void SetSurfaceRunSpeed(double surfaceRunSpeed);

    #endregion
    
    #region Surface run turn speed
    
    public class OnSurfaceRunTurnSpeedChangedArgs
    {
        /// <summary>
        /// New surface run turn speed
        /// </summary>
        public double SurfaceRunTurnSpeed { get; }

        public OnSurfaceRunTurnSpeedChangedArgs(double surfaceRunTurnSpeed)
        {
            SurfaceRunTurnSpeed = surfaceRunTurnSpeed;
        }
    }
    
    /// <summary>
    /// Called when surface run turn speed is changed
    /// </summary>
    delegate void OnSurfaceRunTurnSpeedChangedHandler(object sender, OnSurfaceRunTurnSpeedChangedArgs args);
    
    /// <summary>
    /// Event for surface run speed change
    /// </summary>
    event OnSurfaceRunTurnSpeedChangedHandler OnSurfaceRunTurnSpeedChanged;
    
    /// <summary>
    /// Get current surface run turn speed
    /// </summary>
    double GetSurfaceRunTurnSpeed();

    /// <summary>
    /// Set surface run turn speed
    /// </summary>
    void SetSurfaceRunTurnSpeed(double surfaceRunTurnSpeed);
    
    #endregion

    /// <summary>
    /// Get keyboard keys collection. Use it in dropdowns for action keys
    /// </summary>
    IReadOnlyCollection<Key> GetKeyboardKeysCollection();

    /// <summary>
    /// Get keyboard key index by key
    /// </summary>
    int GetKeyboardKeyIndex(Key key);

    /// <summary>
    /// Get keyboard key by index
    /// </summary>
    Key GetKeyboardKey(int index);

}