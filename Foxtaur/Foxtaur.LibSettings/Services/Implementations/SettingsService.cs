using System.Text.Json;
using Avalonia.Input;
using Foxtaur.LibSettings.Constants;
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
    private Key _surfaceRunTurnRightButton;
    
    public event ISettingsService.OnDemScaleChangedHandler? OnDemScaleChanged;
    public event ISettingsService.OnSurfaceRunSpeedChangedHandler? OnSurfaceRunSpeedChanged;
    public event ISettingsService.OnSurfaceRunTurnSpeedChangedHandler? OnSurfaceRunTurnSpeedChanged;
    public event ISettingsService.OnSurfaceRunForwardButtonChangedHandler? OnSurfaceRunForwardButtonChanged;
    public event ISettingsService.OnSurfaceRunBackwardButtonChangedHandler? OnSurfaceRunBackwardButtonChanged;
    public event ISettingsService.OnSurfaceRunTurnLeftButtonChangedHandler? OnSurfaceRunTurnLeftButtonChanged;
    public event ISettingsService.OnSurfaceRunTurnRightButtonChangedHandler? OnSurfaceRunTurnRightButtonChanged;

    private string _configFilePath;

    public SettingsService()
    {
        // Path to config
        _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SettingsConstants.ConfigFileName);
        
        LoadSettings();
    }
    
    public double GetDemScale()
    {
        return _demScale;
    }

    public void SetDemScale(double demScale)
    {
        _demScale = demScale;
        SaveSettings();
        
        InvokeOnDemScaleChanged();
    }
    
    public double GetSurfaceRunSpeed()
    {
        return _surfaceRunSpeed;
    }

    public void SetSurfaceRunSpeed(double surfaceRunSpeed)
    {
        _surfaceRunSpeed = surfaceRunSpeed;
        SaveSettings();
        
        InvokeOnSurfaceRunSpeedChanged();
    }
    
    public double GetSurfaceRunTurnSpeed()
    {
        return _surfaceRunTurnSpeed;
    }

    public void SetSurfaceRunTurnSpeed(double surfaceRunTurnSpeed)
    {
        _surfaceRunTurnSpeed = surfaceRunTurnSpeed;
        SaveSettings();
        
        InvokeOnSurfaceRunTurnSpeedChanged();
    }

    public Key GetSurfaceRunForwardButton()
    {
        return _surfaceRunForwardButton;
    }

    public void SetSurfaceRunForwardButton(Key surfaceRunForwardButton)
    {
        _surfaceRunForwardButton = surfaceRunForwardButton;
        SaveSettings();
        
        InvokeOnSurfaceRunForwardButtonChanged();
    }
    
    public Key GetSurfaceRunBackwardButton()
    {
        return _surfaceRunBackwardButton;
    }

    public void SetSurfaceRunBackwardButton(Key surfaceRunBackButton)
    {
        _surfaceRunBackwardButton = surfaceRunBackButton;
        SaveSettings();
        
        InvokeOnSurfaceRunBackwardButtonChanged();
    }

    public Key GetSurfaceRunTurnLeftButton()
    {
        return _surfaceRunTurnLeftButton;
    }

    public void SetSurfaceRunTurnLeftButton(Key surfaceRunTurnLeftButton)
    {
        _surfaceRunTurnLeftButton = surfaceRunTurnLeftButton;
        SaveSettings();
        
        InvokeOnSurfaceRunTurnLeftButtonChanged();
    }

    public Key GetSurfaceRunTurnRightButton()
    {
        return _surfaceRunTurnRightButton;
    }

    public void SetSurfaceRunTurnRightButton(Key surfaceRunTurnRightButton)
    {
        _surfaceRunTurnRightButton = surfaceRunTurnRightButton;
        SaveSettings();
        
        InvokeOnSurfaceRunTurnRightButtonChanged();
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
    
    private void InvokeOnSurfaceRunTurnRightButtonChanged()
    {
        OnSurfaceRunTurnRightButtonChanged?.Invoke(this, new ISettingsService.OnSurfaceRunTurnRightButtonChangedArgs(_surfaceRunTurnRightButton));
    }
    
    #region Save and load

    private void SaveSettings()
    {
        var settings = new SerializableSettings()
        {
            DemScale = GetDemScale(),
            SurfaceRunSpeed = GetSurfaceRunSpeed(),
            SurfaceRunTurnSpeed = GetSurfaceRunTurnSpeed(),
            SurfaceRunForwardButton = GetSurfaceRunForwardButton(),
            SurfaceRunBackwardButton = GetSurfaceRunBackwardButton(),
            SurfaceRunTurnLeftButton = GetSurfaceRunTurnLeftButton(),
            SurfaceRunTurnRightButton = GetSurfaceRunTurnRightButton()
        };
        
        var serializedSettings = JsonSerializer.Serialize(settings);
        
        File.WriteAllText(_configFilePath, serializedSettings);
    }

    private void LoadSettings()
    {
        if (!File.Exists(_configFilePath))
        {
            // First run, we need to create config file
            SetDemScale(1.0);
            SetSurfaceRunSpeed(0.000001);
            SetSurfaceRunTurnSpeed(1.0);
            SetSurfaceRunForwardButton(Key.W);
            SetSurfaceRunBackwardButton(Key.S);
            SetSurfaceRunTurnLeftButton(Key.A);
            SetSurfaceRunTurnRightButton(Key.D);

            SaveSettings();
        }
        
        var settings = JsonSerializer.Deserialize<SerializableSettings>(File.ReadAllText(_configFilePath));
        
        SetDemScale(settings.DemScale);
        SetSurfaceRunSpeed(settings.SurfaceRunSpeed);
        SetSurfaceRunTurnSpeed(settings.SurfaceRunTurnSpeed);
        SetSurfaceRunForwardButton(settings.SurfaceRunForwardButton);
        SetSurfaceRunBackwardButton(settings.SurfaceRunBackwardButton);
        SetSurfaceRunTurnLeftButton(settings.SurfaceRunTurnLeftButton);
        SetSurfaceRunTurnRightButton(settings.SurfaceRunTurnRightButton);
    }
    
    #endregion
}