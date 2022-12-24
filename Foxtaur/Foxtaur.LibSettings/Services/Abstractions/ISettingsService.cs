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
        public double NewDemScale { get; }

        public OnDemScaleChangedArgs(double newDemScale)
        {
            NewDemScale = newDemScale;
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
    /// <param name="demScale"></param>
    void SetDemScale(double demScale);

    #endregion

}