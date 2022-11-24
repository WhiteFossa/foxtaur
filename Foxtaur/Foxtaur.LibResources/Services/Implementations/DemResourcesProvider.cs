using Foxtaur.Helpers;
using Foxtaur.LibResources.Models;
using Foxtaur.LibResources.Services.Abstractions;

namespace Foxtaur.LibResources.Services.Implementations;

/// <summary>
/// Provider for DEM resources
/// </summary>
public class DemResourcesProvider : IFragmentedResourcesProvider
{
    private IList<DemFragment> _fragments;

    public DemResourcesProvider()
    {
        _fragments = new List<DemFragment>();

        // 30N-50N stripe
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            -30.000138889f.ToRadians(),
            -0.000138889f.ToRadians(),
            "30n030w_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            -0.000138889f.ToRadians(),
            29.999861111f.ToRadians(),
            "30n000e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            59.999861111f.ToRadians(),
            "30n030e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            59.999861111f.ToRadians(),
            89.999861111f.ToRadians(),
            "30n060e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            89.999861111f.ToRadians(),
            119.999861111f.ToRadians(),
            "30n090e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            119.999861111f.ToRadians(),
            149.999861111f.ToRadians(),
            "30n120e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            149.999861111f.ToRadians(),
            179.999861111f.ToRadians(),
            "30n150e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            -180.000138889f.ToRadians(),
            -150.000138889f.ToRadians(),
            "30n180w_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(49.999861111f.ToRadians(),
            29.999861111f.ToRadians(),
            -150.000138889f.ToRadians(),
            -120.000138889f.ToRadians(),
            "30n150w_20101117_gmted_mea075_lowres.tif",
            true));
    }

    public FragmentedResourceBase GetResource(float lat, float lon)
    {
        var foundFragments = _fragments
            .Where(f => f.IsHit(lat, lon));

        // TODO: Put sorting by resolution here

        return foundFragments.FirstOrDefault();
    }
}