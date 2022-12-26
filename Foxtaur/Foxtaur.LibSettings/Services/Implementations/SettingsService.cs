using Foxtaur.LibSettings.Services.Abstractions;

namespace Foxtaur.LibSettings.Services.Implementations;

public class SettingsService : ISettingsService
{
    private double _demScale;
    private double _surfaceRunSpeed;
    
    public event ISettingsService.OnDemScaleChangedHandler? OnDemScaleChanged;
    public event ISettingsService.OnSurfaceRunSpeedChangedHandler? OnSurfaceRunSpeedChanged;

    public SettingsService()
    {
        // TODO: Load from DB instead
        _demScale = 1.0;
        _surfaceRunSpeed = 0.0000005;
        InvokeOnDemScaleChanged();
    }
    
    public double GetDemScale()
    {
        return _demScale;
    }

    public void SetDemScale(double demScale)
    {
        _demScale = demScale;
        InvokeOnDemScaleChanged();
    }
    
    public double GetSurfaceRunSpeed()
    {
        return _surfaceRunSpeed;
    }

    public void SetSurfaceRunSpeed(double surfaceRunSpeed)
    {
        _surfaceRunSpeed = surfaceRunSpeed;
    }

    private void InvokeOnDemScaleChanged()
    {
        OnDemScaleChanged?.Invoke(this, new ISettingsService.OnDemScaleChangedArgs(_demScale));
    }

    private void InvokeOnSurfaceRunSpeedChanged()
    {
        OnSurfaceRunSpeedChanged?.Invoke(this, new ISettingsService.OnSurfaceRunSpeedChangedArgs(_surfaceRunSpeed));
    }
}