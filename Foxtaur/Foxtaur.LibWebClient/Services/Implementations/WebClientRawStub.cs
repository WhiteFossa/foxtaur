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
        switch (id.ToString().ToUpperInvariant())
        {
            case "6550C9C5-6945-40F1-BDC6-17898C116A32":
                // Davydovo - Start
                return new LocationDto(new Guid("6550C9C5-6945-40F1-BDC6-17898C116A32"), "Start", LocationType.Start, 54.7717312.ToRadians(), -39.8320896.ToRadians(), null);

            case "FEAA7806-7FFC-4CD8-A584-6B41B17A0E77":
                // Davydovo - Fox
                return new LocationDto(new Guid("FEAA7806-7FFC-4CD8-A584-6B41B17A0E77"), "Foxtaurs village", LocationType.Fox, 54.7684903.ToRadians(), -39.8525598.ToRadians(), new Guid("FC7BB34B-F9F0-4E7A-98D1-7699CC1B4423"));

            case "53ECF004-F388-4623-AABC-486BE60B6AC8":
                // Davydovo - Finish
                return new LocationDto(new Guid("53ECF004-F388-4623-AABC-486BE60B6AC8"), "Finish", LocationType.Finish, 54.79184839.ToRadians(), -39.86736020.ToRadians(), null);

            case "D2ADFE4A-38D2-472F-A79C-6D3A6A257B6C":
                // Gorica - Start
                return new LocationDto(new Guid("D2ADFE4A-38D2-472F-A79C-6D3A6A257B6C"), "Start", LocationType.Start, 42.4499615.ToRadians(), -19.2651843.ToRadians(), null);
            
            case "9D448CD1-ADED-43C5-9513-53386548BFCB":
                // Gorica - Fox
                return new LocationDto(new Guid("9D448CD1-ADED-43C5-9513-53386548BFCB"), "Foxtaurs village", LocationType.Fox, 42.4484845.ToRadians(), -19.2744524.ToRadians(), new Guid("FC7BB34B-F9F0-4E7A-98D1-7699CC1B4423"));
            
            case "003062D4-1347-48DA-9193-F90652B09A7E":
                // Gorica - Finish
                return new LocationDto(new Guid("003062D4-1347-48DA-9193-F90652B09A7E"), "Finish", LocationType.Finish, 42.4496250.ToRadians(), -19.2662307.ToRadians(), null);
            
            default:
                throw new ArgumentException("Location not found");
        }
    }

    public async Task<MapDto> GetMapByIdAsync(Guid id)
    {
        switch (id.ToString().ToUpperInvariant())
        {
            case "2754AEB3-9E20-4017-8858-D4E5982D3802":
                return new MapDto(new Guid("2754AEB3-9E20-4017-8858-D4E5982D3802"),
                    "Давыдово",
                    54.807812457.ToRadians(),
                    54.757759918.ToRadians(),
                    -39.879142801.ToRadians(),
                    -39.823302090.ToRadians(),
                    @"Maps/Davydovo/Davydovo.tif.zst");

            case "2947B1E8-E54F-4C47-80E3-1A1E8AC045F7":
                return new MapDto(new Guid("2947B1E8-E54F-4C47-80E3-1A1E8AC045F7"),
                    "Gorica",
                    42.454572697.ToRadians(),
                    42.440712652.ToRadians(),
                    -19.281242689.ToRadians(),
                    -19.262488444.ToRadians(),
                    @"Maps/Gorica/Gorica.tif.zst");

            default:
                throw new ArgumentException("Wrong ID");
        }
    }

    public async Task<DistanceDto> GetDistanceByIdAsync(Guid id)
    {
        switch (id.ToString().ToUpperInvariant())
        {
            case "89E7EC2D-C7E3-42B6-BBB8-C340E681FCBE":
                return new DistanceDto(
                    new Guid("89E7EC2D-C7E3-42B6-BBB8-C340E681FCBE"),
                    "Давыдово",
                    new Guid("2754AEB3-9E20-4017-8858-D4E5982D3802"),
                    true,
                    new Guid("6550C9C5-6945-40F1-BDC6-17898C116A32"),
                    new Guid("53ECF004-F388-4623-AABC-486BE60B6AC8"),
                    new List<Guid> { new Guid("FEAA7806-7FFC-4CD8-A584-6B41B17A0E77") },
                    new List<Guid> { new Guid("E7B81F14-5B4E-446A-9892-36B60AF6511E") }
                );

            case "A59E6C8F-4C5E-47B4-9EF2-8D1B25CD569C":
                return new DistanceDto(
                    new Guid("A59E6C8F-4C5E-47B4-9EF2-8D1B25CD569C"),
                    "Gorica",
                    new Guid("2947B1E8-E54F-4C47-80E3-1A1E8AC045F7"),
                    true,
                    new Guid("D2ADFE4A-38D2-472F-A79C-6D3A6A257B6C"),
                    new Guid("003062D4-1347-48DA-9193-F90652B09A7E"),
                    new List<Guid> { new Guid("9D448CD1-ADED-43C5-9513-53386548BFCB") },
                    new List<Guid> { new Guid("E7B81F14-5B4E-446A-9892-36B60AF6511E") }
                );
                break;
            
            default:
                throw new ArgumentException("Wrong ID");
        }
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
            ),
            
            new DistanceDto(
                new Guid("A59E6C8F-4C5E-47B4-9EF2-8D1B25CD569C"),
                "Gorica",
                new Guid("2947B1E8-E54F-4C47-80E3-1A1E8AC045F7"),
                true,
                new Guid("D2ADFE4A-38D2-472F-A79C-6D3A6A257B6C"),
                new Guid("003062D4-1347-48DA-9193-F90652B09A7E"),
                new List<Guid> { new Guid("9D448CD1-ADED-43C5-9513-53386548BFCB") },
                new List<Guid> { new Guid("E7B81F14-5B4E-446A-9892-36B60AF6511E") }
            )
        };
    }
}