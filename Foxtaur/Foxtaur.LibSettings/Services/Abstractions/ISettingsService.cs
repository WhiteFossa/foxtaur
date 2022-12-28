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

    #region Surface run forward button
    
    public class OnSurfaceRunForwardButtonChangedArgs
    {
        /// <summary>
        /// New forward button
        /// </summary>
        public Key SurfaceRunForwardButton { get; }

        public OnSurfaceRunForwardButtonChangedArgs(Key surfaceRunForwardButton)
        {
            SurfaceRunForwardButton = surfaceRunForwardButton;
        }
    }
    
    /// <summary>
    /// Called when surface run forward button is changed
    /// </summary>
    delegate void OnSurfaceRunForwardButtonChangedHandler(object sender, OnSurfaceRunForwardButtonChangedArgs args);
    
    /// <summary>
    /// Event for surface run forward button change
    /// </summary>
    event OnSurfaceRunForwardButtonChangedHandler OnSurfaceRunForwardButtonChanged;
    
    /// <summary>
    /// Get current surface run forward button
    /// </summary>
    Key GetSurfaceRunForwardButton();

    /// <summary>
    /// Set surface run forward button
    /// </summary>
    void SetSurfaceRunForwardButton(Key surfaceRunForwardButton);
    
    #endregion
    
    #region Surface run backward button
    
    public class OnSurfaceRunBackwardButtonChangedArgs
    {
        /// <summary>
        /// New backward button
        /// </summary>
        public Key SurfaceRunBackwardButton { get; }

        public OnSurfaceRunBackwardButtonChangedArgs(Key surfaceRunBackwardButton)
        {
            SurfaceRunBackwardButton = surfaceRunBackwardButton;
        }
    }
    
    /// <summary>
    /// Called when surface run back button is changed
    /// </summary>
    delegate void OnSurfaceRunBackwardButtonChangedHandler(object sender, OnSurfaceRunBackwardButtonChangedArgs args);
    
    /// <summary>
    /// Event for surface run back button change
    /// </summary>
    event OnSurfaceRunBackwardButtonChangedHandler OnSurfaceRunBackwardButtonChanged;
    
    /// <summary>
    /// Get current surface run back button
    /// </summary>
    Key GetSurfaceRunBackwardButton();

    /// <summary>
    /// Set surface run backward button
    /// </summary>
    void SetSurfaceRunBackwardButton(Key surfaceRunBackButton);
    
    #endregion
    
    #region Surface run turn left button
    
    public class OnSurfaceRunTurnLeftButtonChangedArgs
    {
        /// <summary>
        /// New turn left button
        /// </summary>
        public Key SurfaceRunTurnLeftButton { get; }

        public OnSurfaceRunTurnLeftButtonChangedArgs(Key surfaceRunTurnLeftButton)
        {
            SurfaceRunTurnLeftButton = surfaceRunTurnLeftButton;
        }
    }
    
    /// <summary>
    /// Called when surface run turn left button is changed
    /// </summary>
    delegate void OnSurfaceRunTurnLeftButtonChangedHandler(object sender, OnSurfaceRunTurnLeftButtonChangedArgs args);
    
    /// <summary>
    /// Event for surface run turn left button change
    /// </summary>
    event OnSurfaceRunTurnLeftButtonChangedHandler OnSurfaceRunTurnLeftButtonChanged;
    
    /// <summary>
    /// Get current surface run turn left button
    /// </summary>
    Key GetSurfaceRunTurnLeftButton();

    /// <summary>
    /// Set surface run turn left button
    /// </summary>
    void SetSurfaceRunTurnLeftButton(Key surfaceRunTurnLeftButton);
    
    #endregion
    
    #region Surface run turn right button
    
    public class OnSurfaceRunTurnRightButtonChangedArgs
    {
        /// <summary>
        /// New turn right button
        /// </summary>
        public Key SurfaceRunTurnRightButton { get; }

        public OnSurfaceRunTurnRightButtonChangedArgs(Key surfaceRunTurnRightButton)
        {
            SurfaceRunTurnRightButton = surfaceRunTurnRightButton;
        }
    }
    
    /// <summary>
    /// Called when surface run turn right button is changed
    /// </summary>
    delegate void OnSurfaceRunTurnRightButtonChangedHandler(object sender, OnSurfaceRunTurnRightButtonChangedArgs args);
    
    /// <summary>
    /// Event for surface run turn right button change
    /// </summary>
    event OnSurfaceRunTurnRightButtonChangedHandler OnSurfaceRunTurnRightButtonChanged;
    
    /// <summary>
    /// Get current surface run turn right button
    /// </summary>
    Key GetSurfaceRunTurnRightButton();

    /// <summary>
    /// Set surface run turn left button
    /// </summary>
    void SetSurfaceRunTurnRightButton(Key surfaceRunTurnRightButton);
    
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