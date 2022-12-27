using Avalonia.Input;
using Foxtaur.LibSettings.Services.Abstractions;

namespace Foxtaur.LibSettings.Services.Implementations;

public class SettingsService : ISettingsService
{
    private double _demScale;
    private double _surfaceRunSpeed;
    private double _surfaceRunTurnSpeed;
    private Key _surfaceRunForwardButton;
    private Key _surfaceRunBackwardButton;
    private Key _surfaceRunTurnLeftButton;
    
    public event ISettingsService.OnDemScaleChangedHandler? OnDemScaleChanged;
    public event ISettingsService.OnSurfaceRunSpeedChangedHandler? OnSurfaceRunSpeedChanged;
    public event ISettingsService.OnSurfaceRunTurnSpeedChangedHandler? OnSurfaceRunTurnSpeedChanged;
    public event ISettingsService.OnSurfaceRunForwardButtonChangedHandler? OnSurfaceRunForwardButtonChanged;
    public event ISettingsService.OnSurfaceRunBackwardButtonChangedHandler? OnSurfaceRunBackwardButtonChanged;
    public event ISettingsService.OnSurfaceRunTurnLeftButtonChangedHandler? OnSurfaceRunTurnLeftButtonChanged;

    public SettingsService()
    {
        // TODO: Load from DB instead
        _demScale = 1.0;
        _surfaceRunSpeed = 0.0000005;
        _surfaceRunTurnSpeed = 1.0;
        _surfaceRunForwardButton = Key.W;
        _surfaceRunBackwardButton = Key.S;
        _surfaceRunTurnLeftButton = Key.A;
        
        InvokeOnDemScaleChanged();
        InvokeOnSurfaceRunSpeedChanged();
        InvokeOnSurfaceRunTurnSpeedChanged();
        InvokeOnSurfaceRunForwardButtonChanged();
        InvokeOnSurfaceRunBackwardButtonChanged();
        InvokeOnSurfaceRunTurnLeftButtonChanged();
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

    public Key GetSurfaceRunForwardButton()
    {
        return _surfaceRunForwardButton;
    }

    public void SetSurfaceRunForwardButton(Key surfaceRunForwardButton)
    {
        _surfaceRunForwardButton = surfaceRunForwardButton;
        InvokeOnSurfaceRunForwardButtonChanged();
    }
    
    public Key GetSurfaceRunBackwardButton()
    {
        return _surfaceRunBackwardButton;
    }

    public void SetSurfaceRunBackwardButton(Key surfaceRunBackButton)
    {
        _surfaceRunBackwardButton = surfaceRunBackButton;
        InvokeOnSurfaceRunBackwardButtonChanged();
    }

    public Key GetSurfaceRunTurnLeftButton()
    {
        return _surfaceRunTurnLeftButton;
    }

    public void SetSurfaceRunTurnLeftButton(Key surfaceRunTurnLeftButton)
    {
        _surfaceRunTurnLeftButton = surfaceRunTurnLeftButton;
        InvokeOnSurfaceRunTurnLeftButtonChanged();
    }

    public IReadOnlyCollection<Key> GetKeyboardKeysCollection()
    {
        return new List<Key>()
        {
            Key.A,
            Key.B,
            Key.C,
            Key.D,
            Key.E,
            Key.F,
            Key.G,
            Key.H,
            Key.I,
            Key.J,
            Key.K,
            Key.L,
            Key.M,
            Key.N,
            Key.O,
            Key.P,
            Key.Q,
            Key.R,
            Key.S,
            Key.T,
            Key.U,
            Key.V,
            Key.W,
            Key.X,
            Key.Y,
            Key.Z
        };
    }

    public int GetKeyboardKeyIndex(Key key)
    {
        var result = GetKeyboardKeysCollection()
            .ToList()
            .IndexOf(key);

        if (result == -1)
        {
            throw new ArgumentException(nameof(key));
        }

        return result;
    }

    public Key GetKeyboardKey(int index)
    {
        var keys = GetKeyboardKeysCollection();

        if (index < 0 || index >= keys.Count)
        {
            throw new ArgumentException(nameof(index));
        }

        return keys.ToList()[index];
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
    
    private void InvokeOnSurfaceRunForwardButtonChanged()
    {
        OnSurfaceRunForwardButtonChanged?.Invoke(this, new ISettingsService.OnSurfaceRunForwardButtonChangedArgs(_surfaceRunForwardButton));
    }
    
    private void InvokeOnSurfaceRunBackwardButtonChanged()
    {
        OnSurfaceRunBackwardButtonChanged?.Invoke(this, new ISettingsService.OnSurfaceRunBackwardButtonChangedArgs(_surfaceRunBackwardButton));
    }
    
    private void InvokeOnSurfaceRunTurnLeftButtonChanged()
    {
        OnSurfaceRunTurnLeftButtonChanged?.Invoke(this, new ISettingsService.OnSurfaceRunTurnLeftButtonChangedArgs(_surfaceRunTurnLeftButton));
    }
}