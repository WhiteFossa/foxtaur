using Foxtaur.LibWebClient.Enums;
using Foxtaur.LibWebClient.Models;
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
}