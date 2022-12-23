using Foxtaur.LibSettings.Services.Abstractions;

namespace Foxtaur.LibSettings.Services.Implementations;

public class SettingsService : ISettingsService
{
    private double _demScale;
    
    public event ISettingsService.OnDemScaleChangedHandler? OnDemScaleChanged;

    public SettingsService()
    {
        // TODO: Load from DB instead
        _demScale = 1.0;
        //InvokeOnDemScaleChanged();
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

    private void InvokeOnDemScaleChanged()
    {
        OnDemScaleChanged?.Invoke(this, new ISettingsService.OnDemScaleChangedArgs(_demScale));
    }
}