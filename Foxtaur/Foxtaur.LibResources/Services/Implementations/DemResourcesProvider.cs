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
        _fragments.Add(new DemFragment(50.0f.ToRadians(),
            30.0f.ToRadians(),
            -30.0f.ToRadians(),
            0.0f.ToRadians(),
            "30n030w_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(50.0f.ToRadians(),
            30.0f.ToRadians(),
            0.0f.ToRadians(),
            30.0f.ToRadians(),
            "30n000e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(50.0f.ToRadians(),
            30.0f.ToRadians(),
            30.0f.ToRadians(),
            60.0f.ToRadians(),
            "30n030e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(50.0f.ToRadians(),
            30.0f.ToRadians(),
            60.0f.ToRadians(),
            90.0f.ToRadians(),
            "30n060e_20101117_gmted_mea075_lowres.tif",
            true));
        
        _fragments.Add(new DemFragment(50.0f.ToRadians(),
            30.0f.ToRadians(),
            90.0f.ToRadians(),
            120.0f.ToRadians(),
            "30n090e_20101117_gmted_mea075_lowres.tif",
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