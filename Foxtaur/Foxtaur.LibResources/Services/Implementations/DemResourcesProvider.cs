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

        _fragments.Add(new DemFragment(50.0f.ToRadians(),
            30.0f.ToRadians(),
            0.0f.ToRadians(),
            30.0f.ToRadians(),
            "30n000e_20101117_gmted_mea075_lowres.tif",
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