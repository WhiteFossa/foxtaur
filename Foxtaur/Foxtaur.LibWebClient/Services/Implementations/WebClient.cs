using Foxtaur.LibWebClient.Enums;
using Foxtaur.LibWebClient.Models;
using Foxtaur.LibWebClient.Models.DTOs;
using Foxtaur.LibWebClient.Services.Abstract;

namespace Foxtaur.LibWebClient.Services.Implementations;

public class WebClient : IWebClient
{
    private IWebClientRaw _client;

    public WebClient(IWebClientRaw webClient)
    {
        _client = webClient;
    }
    
    public async Task<IReadOnlyCollection<Distance>> GetDistancesWithoutIncludeAsync()
    {
        var distances = await _client.ListDistancesAsync();

        var mapsIds = distances
            .Select(d => d.MapId);

        var maps = new List<Map>();
        foreach (var mapId in mapsIds)
        {
            var mapDto = await _client.GetMapByIdAsync(mapId);
            maps.Add(new Map(mapDto.Id, mapDto.Name, mapDto.NorthLat, mapDto.SouthLat, mapDto.EastLon, mapDto.WestLon, mapDto.Url));
        }

        return distances
            .Select(d =>
            {
                return new Distance(
                    d.Id,
                    d.Name,
                    maps.FirstOrDefault(m => m.Id == d.MapId),
                    d.IsActive,
                    new Location(Guid.NewGuid(), "Invalid start location", LocationType.Start, 0, 0, null),
                    new Location(Guid.NewGuid(), "Invalid finish location", LocationType.Start, 0, 0, null),
                    new List<Fox>(),
                    new List<Hunter>()
                );
            })
            .ToList();
    }

    public async Task<Distance> GetDistanceByIdAsync(Guid distanceId)
    {
        var distanceDto = await _client.GetDistanceByIdAsync(distanceId);
        if (distanceDto == null)
        {
            throw new ArgumentException(nameof(distanceId));
        }

        var mapDto = await _client.GetMapByIdAsync(distanceDto.MapId);
        if (mapDto == null)
        {
            throw new InvalidOperationException($"Map with ID={ distanceDto.MapId } is not found!");
        }

        var startDto = await _client.GetLocationByIdAsync(distanceDto.StartLocationId);
        if (startDto == null)
        {
            throw new InvalidCastException($"Start location (ID={ distanceDto.StartLocationId }) is not found!");
        }
        
        var finishDto = await _client.GetLocationByIdAsync(distanceDto.FinishLocationId);
        if (finishDto == null)
        {
            throw new InvalidCastException($"Finish location (ID={ distanceDto.FinishLocationId }) is not found!");
        }

        // Foxes locations
        var foxesLocationsIds = distanceDto
            .FoxesLocationsIds;

        var foxesLocationsDtos = new List<LocationDto>();
        foreach (var foxLocationId in foxesLocationsIds)
        {
            foxesLocationsDtos.Add(await _client.GetLocationByIdAsync(foxLocationId));
        }
        
        // Foxes
        var foxesIds = foxesLocationsDtos
            .Select(fl => fl.FoxId);

        var foxesDtos = new List<FoxDto>();
        foreach (var foxId in foxesIds)
        {
            foxesDtos.Add(await _client.GetFoxByIdAsync(foxId.Value));
        }
        
        // Hunters
        var huntersIds = distanceDto
            .HuntersIds;

        var huntersDtos = new List<HunterDto>();
        foreach (var hunterId in huntersIds)
        {
            huntersDtos.Add(await _client.GetHunterByIdAsync(hunterId));
        }
        
        // Teams
        var teamsIds = huntersDtos
            .Select(h => h.TeamId);

        var teamsDtos = new List<TeamDto>();
        foreach (var teamId in teamsIds)
        {
            if (teamId.HasValue)
            {
                teamsDtos.Add(await _client.GetTeamByIdAsync(teamId.Value));
            }
        }

        return new Distance(
            distanceDto.Id,
            distanceDto.Name,
            new Map(mapDto.Id, mapDto.Name, mapDto.NorthLat, mapDto.SouthLat, mapDto.EastLon, mapDto.WestLon, mapDto.Url),
            distanceDto.IsActive,
            new Location(startDto.Id, startDto.Name, startDto.Type, startDto.Lat, startDto.Lon, null),
            new Location(finishDto.Id, finishDto.Name, finishDto.Type, finishDto.Lat, finishDto.Lon, null),
            foxesDtos.Select(f => new Fox(f.Id, f.Name, f.Frequency, f.Code)).ToList(),
            huntersDtos.Select(h =>
            {
                var teamDto = teamsDtos
                    .FirstOrDefault(td => td.Id == h.TeamId);
                var team = teamDto != null ? new Team(teamDto.Id, teamDto.Name) : null;
                
                return new Hunter(h.Id, h.Name, h.IsRunning, team, h.Lat, h.Lon);
            }).ToList());
    }
}