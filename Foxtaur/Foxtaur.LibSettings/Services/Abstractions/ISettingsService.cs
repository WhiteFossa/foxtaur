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

}