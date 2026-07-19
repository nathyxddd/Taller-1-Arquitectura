using Shortly.Domain.Entities;

namespace Shortly.Application.DTOs;

public class LinkResponse
{
    public long Id { get; set; }
    public string Url { get; set; } = null!;
    public string ShortUrl { get; set; } = null!;
    public int Clicks { get; set; }
    public static LinkResponse From(Link link) => new()
    {
        Id = link.Id,
        Url = link.Url,
        ShortUrl = link.ShortUrl,
        Clicks = link.Clicks
    };
}
