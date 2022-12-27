using Foxtaur.LibSettings.Services.Abstractions;

namespace Foxtaur.LibSettings.Services.Implementations;

public class SettingsService : ISettingsService
{
    private double _demScale;
    private double _surfaceRunSpeed;
    private double _surfaceRunTurnSpeed;
    
    public event ISettingsService.OnDemScaleChangedHandler? OnDemScaleChanged;
    public event ISettingsService.OnSurfaceRunSpeedChangedHandler? OnSurfaceRunSpeedChanged;
    public event ISettingsService.OnSurfaceRunTurnSpeedChangedHandler? OnSurfaceRunTurnSpeedChanged;

    public SettingsService()
    {
        // TODO: Load from DB instead
        _demScale = 1.0;
        _surfaceRunSpeed = 0.0000005;
        _surfaceRunTurnSpeed = 1.0;
        
        InvokeOnDemScaleChanged();
        InvokeOnSurfaceRunSpeedChanged();
        InvokeOnSurfaceRunTurnSpeedChanged();
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
        InvokeOnSurfaceRunSpeedChanged();
    }
    
    public double GetSurfaceRunTurnSpeed()
    {
        return _surfaceRunTurnSpeed;
    }

    public void SetSurfaceRunTurnSpeed(double surfaceRunTurnSpeed)
    {
        _surfaceRunTurnSpeed = surfaceRunTurnSpeed;
        InvokeOnSurfaceRunTurnSpeedChanged();
    }

    private void InvokeOnDemScaleChanged()
    {
        OnDemScaleChanged?.Invoke(this, new ISettingsService.OnDemScaleChangedArgs(_demScale));
    }

    private void InvokeOnSurfaceRunSpeedChanged()
    {
        OnSurfaceRunSpeedChanged?.Invoke(this, new ISettingsService.OnSurfaceRunSpeedChangedArgs(_surfaceRunSpeed));
    }
    
    private void InvokeOnSurfaceRunTurnSpeedChanged()
    {
        OnSurfaceRunTurnSpeedChanged?.Invoke(this, new ISettingsService.OnSurfaceRunTurnSpeedChangedArgs(_surfaceRunTurnSpeed));
    }
}