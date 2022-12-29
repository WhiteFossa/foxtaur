using Foxtaur.Helpers;
using Foxtaur.LibWebClient.Enums;
using Foxtaur.LibWebClient.Models.DTOs;
using Foxtaur.LibWebClient.Services.Abstract;

namespace Foxtaur.LibWebClient.Services.Implementations;

public class WebClientRawStub : IWebClientRaw
{
    public async Task<TeamDto> GetTeamByIdAsync(Guid id)
    {
        var teamId = new Guid("AE9EE155-BCDC-44C3-B83F-A4837A3EC443");

        if (id != teamId)
        {
            throw new ArgumentException("Team not found!");
        }

        return new TeamDto(teamId, "Fox yiffers");
    }

    public async Task<HunterDto> GetHunterByIdAsync(Guid id)
    {
        var hunterId = new Guid("E7B81F14-5B4E-446A-9892-36B60AF6511E");

        if (id != hunterId)
        {
            throw new ArgumentException("Hunter not found!");
        }

        return new HunterDto(hunterId, "Garrek", true, new Guid("AE9EE155-BCDC-44C3-B83F-A4837A3EC443"), 54.777324.ToRadians(),-39.849310.ToRadians());
    }

    public async Task<FoxDto> GetFoxByIdAsync(Guid id)
    {
        var foxId = new Guid("FC7BB34B-F9F0-4E7A-98D1-7699CC1B4423");
        
        if (id != foxId)
        {
            throw new ArgumentException("Fox not found!");
        }

        return new FoxDto(foxId, "Malena", 145000000, "MOE");
    }

    public async Task<LocationDto> GetLocationByIdAsync(Guid id)
    {
        var startLocationId = new Guid("6550C9C5-6945-40F1-BDC6-17898C116A32");
        var foxLocationId = new Guid("FEAA7806-7FFC-4CD8-A584-6B41B17A0E77");
        var finishLocationId = new Guid("53ECF004-F388-4623-AABC-486BE60B6AC8");

        
        switch (id.ToString())
        {
            case "6550C9C5-6945-40F1-BDC6-17898C116A32":
                // Start
                return new LocationDto(startLocationId, "Start", LocationType.Start, 54.7717312.ToRadians(), -39.8320896.ToRadians(), null);
                break;
            
            case "FEAA7806-7FFC-4CD8-A584-6B41B17A0E77":
                // Fox
                return new LocationDto(foxLocationId, "Foxtaurs village", LocationType.Fox, 54.7684903.ToRadians(), -39.8525598.ToRadians(), new Guid("FC7BB34B-F9F0-4E7A-98D1-7699CC1B4423"));
                break;
            
            case "53ECF004-F388-4623-AABC-486BE60B6AC8":
                // Finish
                return new LocationDto(finishLocationId, "Finish", LocationType.Finish, 54.79184839.ToRadians(), -39.86736020.ToRadians(), null);
                break;
            
            default:
                throw new ArgumentException("Location not found");
        }
    }

    public async Task<MapDto> GetMapByIdAsync(Guid id)
    {
        var mapId = new Guid("2754AEB3-9E20-4017-8858-D4E5982D3802");
        
        if (id != mapId)
        {
            throw new ArgumentException("Map not found!");
        }

        return new MapDto(mapId,
            "Давыдово",
            54.807812457.ToRadians(),
            54.757759918.ToRadians(),
            -39.879142801.ToRadians(),
            -39.823302090.ToRadians(),
            @"Maps/Davydovo/Davydovo.tif.zst");
    }

    public async Task<DistanceDto> GetDistanceByIdAsync(Guid id)
    {
        var distanceId = new Guid("89E7EC2D-C7E3-42B6-BBB8-C340E681FCBE");

        if (id != distanceId)
        {
            throw new ArgumentException("Distance not found!");
        }

        return new DistanceDto(
            distanceId,
            "Давыдово",
            new Guid("2754AEB3-9E20-4017-8858-D4E5982D3802"),
            true,
            new Guid("6550C9C5-6945-40F1-BDC6-17898C116A32"),
            new Guid("53ECF004-F388-4623-AABC-486BE60B6AC8"),
            new List<Guid> { new Guid("FEAA7806-7FFC-4CD8-A584-6B41B17A0E77") },
            new List<Guid> { new Guid("E7B81F14-5B4E-446A-9892-36B60AF6511E") }
        );
    }

    public async Task<IReadOnlyCollection<DistanceDto>> ListDistancesAsync()
    {
        return new List<DistanceDto>()
        {
            new DistanceDto(
                new Guid("89E7EC2D-C7E3-42B6-BBB8-C340E681FCBE"),
                "Давыдово",
                new Guid("2754AEB3-9E20-4017-8858-D4E5982D3802"),
                true,
                new Guid("6550C9C5-6945-40F1-BDC6-17898C116A32"),
                new Guid("53ECF004-F388-4623-AABC-486BE60B6AC8"),
                new List<Guid> { new Guid("FEAA7806-7FFC-4CD8-A584-6B41B17A0E77") },
                new List<Guid> { new Guid("E7B81F14-5B4E-446A-9892-36B60AF6511E") }
            )
        };
    }
}