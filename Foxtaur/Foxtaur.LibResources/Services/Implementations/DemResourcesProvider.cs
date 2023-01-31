using System.Timers;
using Foxtaur.Helpers;
using Foxtaur.LibResources.Constants;
using Foxtaur.LibResources.Enums;
using Foxtaur.LibResources.Models;
using Foxtaur.LibResources.Services.Abstractions;
using NLog;
using Timer = System.Timers.Timer;

namespace Foxtaur.LibResources.Services.Implementations;

/// <summary>
/// Provider for DEM resources
/// </summary>
public class DemResourcesProvider : IFragmentedResourcesProvider
{
    private IList<DemFragment> _fragments;
    
    /// <summary>
    /// Cache clear timer
    /// </summary>
    private Timer _cacheClearTimer;
    
    private Logger _logger = LogManager.GetCurrentClassLogger();

    private object _clearCacheLock = new object();
    
    private DemFragment _lastHitFragment = null;
    
    private Mutex _getResourceLock = new Mutex();

    public DemResourcesProvider()
    {
        _fragments = new List<DemFragment>();

        #region Zoom levels 0 and 1

        // 70S-50S stripe
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s000e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s030e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s060e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s090e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s120e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s150e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s180w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s150w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s120w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s090w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s060w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70s030w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        // 50S-30S stripe
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s000e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s030e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s060e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s090e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s120e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s150e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s180w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s150w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s120w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s090w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s060w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50s030w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        // 30S-10S stripe
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s000e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s030e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s060e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s090e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s120e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s150e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s180w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s150w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s120w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s090w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s060w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30s030w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        // 10S-10N stripe
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s000e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s030e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s060e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s090e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s120e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s150e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s180w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s150w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s120w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s090w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s060w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10s030w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        // 10N-30N stripe
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n000e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n030e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n060e_20101117_gmted_mea075_lowres.tif.zst",
            false));
            
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n090e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n120e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n150e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n180w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n150w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n120w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n090w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n060w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "10n030w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        // 30N-50N stripe
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n000e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n030e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n060e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n090e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n120e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n150e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n180w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n150w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n120w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n090w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n060w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "30n030w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        // 50N-70N stripe
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n000e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n030e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n060e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n090e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n120e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n150e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n180w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n150w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n120w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n090w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n060w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "50n030w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        // 70N-90N stripe
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n000e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n030e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n060e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n090e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n120e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n150e_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n180w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n150w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n120w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n090w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n060w_20101117_gmted_mea075_lowres.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel0, ZoomLevel.ZoomLevel1 },
            ResourcesConstants.DemRemotePath + "70n030w_20101117_gmted_mea075_lowres.tif.zst",
            false));

        #endregion
        
        #region Zoom level 2 and surface run

        // 70S-50S stripe
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s000e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s030e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s060e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s090e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s120e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s150e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s180w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s150w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s120w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s090w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s060w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-50.000138889.ToRadians(),
            -70.000138889.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70s030w_20101117_gmted_mea075.tif.zst",
            false));
        
        // 50S-30S stripe
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s000e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s030e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s060e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s090e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s120e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s150e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s180w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s150w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s120w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s090w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s060w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-30.000138889.ToRadians(),
            -50.000138889.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50s030w_20101117_gmted_mea075.tif.zst",
            false));
        
        // 30S-10S stripe
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s000e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s030e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s060e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s090e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s120e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s150e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s180w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s150w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s120w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s090w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s060w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(-10.000138889.ToRadians(),
            -30.000138889.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30s030w_20101117_gmted_mea075.tif.zst",
            false));
        
        // 10S-10N stripe
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s000e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s030e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s060e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s090e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s120e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s150e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s180w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s150w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s120w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s090w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s060w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(9.999861111.ToRadians(),
            -10.000138889.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10s030w_20101117_gmted_mea075.tif.zst",
            false));
        
        // 10N-30N stripe
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n000e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n030e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n060e_20101117_gmted_mea075.tif.zst",
            false));
            
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n090e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n120e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n150e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n180w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n150w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n120w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n090w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n060w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(29.999861111.ToRadians(),
            9.999861111.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "10n030w_20101117_gmted_mea075.tif.zst",
            false));
        
        // 30N-50N stripe
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n000e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n030e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n060e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n090e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n120e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n150e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n180w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n150w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n120w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n090w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n060w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(49.999861111.ToRadians(),
            29.999861111.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "30n030w_20101117_gmted_mea075.tif.zst",
            false));
        
        // 50N-70N stripe
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n000e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n030e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n060e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n090e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n120e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n150e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n180w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n150w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n120w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n090w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n060w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(69.999861111.ToRadians(),
            49.999861111.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "50n030w_20101117_gmted_mea075.tif.zst",
            false));
        
        // 70N-90N stripe
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            0.000138889.ToRadians(),
            -29.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n000e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -29.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n030e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -59.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n060e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -89.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n090e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -119.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n120e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            -149.999861111.ToRadians(),
            -179.999861111.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n150e_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            180.000138889.ToRadians(),
            150.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n180w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            150.000138889.ToRadians(),
            120.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n150w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            120.000138889.ToRadians(),
            90.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n120w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            90.000138889.ToRadians(),
            60.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n090w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            60.000138889.ToRadians(),
            30.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n060w_20101117_gmted_mea075.tif.zst",
            false));
        
        _fragments.Add(new DemFragment(89.999861111.ToRadians(),
            69.999861111.ToRadians(),
            30.000138889.ToRadians(),
            0.000138889.ToRadians(),
            new ZoomLevel[] { ZoomLevel.ZoomLevel2, ZoomLevel.ZoomLevelSurfaceRun },
            ResourcesConstants.DemRemotePath + "70n030w_20101117_gmted_mea075.tif.zst",
            false));

        #endregion
        
        // Starting cache clear timer
        _cacheClearTimer = new Timer(1000);
        _cacheClearTimer.Elapsed += OnCacheClearTimer;
        _cacheClearTimer.AutoReset = true;
        _cacheClearTimer.Enabled = true;

    }

    private void OnCacheClearTimer(object? sender, ElapsedEventArgs e)
    {
        lock (_clearCacheLock)
        {
            var loadedFragments = _fragments
                .Where(f => f.IsLoaded);

            var size = loadedFragments
                .Sum(lf => lf.CacheSize);

            // Cleaning one fragment per timer call
            if (size > ResourcesConstants.MaxDemCacheSize)
            {
                var orderedFragments = loadedFragments
                    .Where(lf => !lf.ZoomLevels.Contains(ZoomLevel.ZoomLevel0)) // We can unload only high-resolution fragments
                    .Where(lf => !lf.ZoomLevels.Contains(ZoomLevel.ZoomLevel1))
                    .OrderBy(lf => lf.LastAccessTime);

                orderedFragments
                    .FirstOrDefault()
                    .Unload();
            }   
        }
    }

    public FragmentedResourceBase GetResource(double lat, double lon, ZoomLevel zoomLevel)
    {
        _getResourceLock.WaitOne();

        try
        {
            if (_lastHitFragment != null && _lastHitFragment.IsHit(lat, lon) && FastContains(_lastHitFragment.ZoomLevels, zoomLevel))
            {
                return _lastHitFragment; // Avoid full scan
            }

            _lastHitFragment = _fragments
                .Where(f => f.IsHit(lat, lon))
                .FirstOrDefault(f => FastContains(f.ZoomLevels, zoomLevel));

            return _lastHitFragment;
        }
        finally
        {
            _getResourceLock.ReleaseMutex();
        }
    }

    private bool FastContains(ZoomLevel[] levels, ZoomLevel level)
    {
        if (levels.Length == 0)
        {
            return false;
        }
        else if (levels.Length == 1 && levels[0] == level)
        {
            return true;
        }
        else if (levels.Length == 2 && (levels[0] == level || levels[1] == level))
        {
            return true;
        }

        return false;
    }
}