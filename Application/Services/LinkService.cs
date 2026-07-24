using System.Security.Cryptography;
using System.Text;
using Shortly.Application.DTOs;
using Shortly.Application.Interfaces;
using Shortly.Domain.Entities;

namespace Shortly.Application.Services;

public sealed class LinkService : ILinkService
{
    private readonly ILogger<LinkService> _logger;
    private readonly ILinkRepository _linkRepository;

    public LinkService(ILinkRepository linkRepository, ILogger<LinkService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));
    }

    public async Task<LinkResponse> CreateLink(string url, long userId)
    {
        _logger.LogDebug("Creating link for URL: {Url} and userId: {UserId}", url, userId);

        var shortUrl = GenerateSecureShortUrl();
        var link = new Link(url, shortUrl, userId);

        await _linkRepository.AddAsync(link);
        await _linkRepository.SaveChangesAsync();

        _logger.LogInformation("Link created successfully with shortUrl: {ShortUrl} and id: {Id}.", link.ShortUrl, link.Id);
        return LinkResponse.From(link);
    }

    /// <summary>
    /// Genera un token corto seguro de 12 caracteres a partir de un ULID, aplicando la función de hash criptográfica SHA-256 codificada en Base62.
    /// 
    /// Fundamentos de Seguridad y Privacidad:
    /// 1. Prevención de Ataques de Enumeración de Recursos (Resource Enumeration): Los tokens secuenciales o basados en tiempo permiten
    ///    a un atacante deducir la cantidad total de enlaces creados y predecir o listar otros recursos en el sistema.
    /// 2. Mitigación de Filtración de Metadata Temporal: Los ULID exponen en sus primeros 48 bits la marca de tiempo exacta de creación.
    ///    Al procesar el ULID con SHA-256, se transforma el valor temporal en un hash digest de sentido único e impredecible, garantizando
    ///    privacidad y unicidad sin revelar cuándo se generó el recurso.
    /// </summary>
    private static string GenerateSecureShortUrl()
    {
        var rawUlid = Ulid.NewUlid().ToString();
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawUlid));

        const string base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var number = System.Numerics.BigInteger.Abs(new System.Numerics.BigInteger(hashBytes));
        var sb = new StringBuilder();

        while (number > 0)
        {
            number = System.Numerics.BigInteger.DivRem(number, 62, out var remainder);
            sb.Append(base62Chars[(int)remainder]);
        }

        var base62String = sb.ToString();
        return base62String.Length >= 12 ? base62String[..12] : base62String.PadRight(12, '0');
    }

    public async Task<LinkResponse> IncrementClicks(long linkId)
    {
        _logger.LogDebug("Incrementing clicks for linkId: {LinkId}", linkId);

        var link = await _linkRepository.GetByIdAsync(linkId);
        if (link is null)
        {
            _logger.LogWarning("IncrementClicks failed: No link found with id {LinkId}.", linkId);
            throw new KeyNotFoundException($"No link found with id '{linkId}'.");
        }

        link.IncrementClicks();
        await _linkRepository.SaveChangesAsync();

        _logger.LogInformation("Clicks incremented for linkId: {LinkId}. Total clicks: {Clicks}.", link.Id, link.Clicks);
        return LinkResponse.From(link);
    }

    public async Task<LinkResponse> GetLink(string shortUrl)
    {
        _logger.LogDebug("Retrieving link with shortUrl: {ShortUrl}", shortUrl);

        var link = await _linkRepository.GetByShortUrlAsync(shortUrl);
        if (link is null)
        {
            _logger.LogWarning("Link not found with shortUrl {ShortUrl}.", shortUrl);
            throw new KeyNotFoundException($"No link found with shortUrl '{shortUrl}'.");
        }

        _logger.LogInformation("Link retrieved successfully with shortUrl: {ShortUrl} and id: {Id}.", link.ShortUrl, link.Id);
        return LinkResponse.From(link);
    }

    public async Task<LinkResponse> GetLinkById(long id)
    {
        _logger.LogDebug("Retrieving link with id: {Id}", id);

        var link = await _linkRepository.GetByIdAsync(id);
        if (link is null)
        {
            _logger.LogWarning("Link not found with id {Id}.", id);
            throw new KeyNotFoundException($"No link found with id '{id}'.");
        }

        _logger.LogInformation("Link retrieved successfully with id: {Id}.", link.Id);
        return LinkResponse.From(link);
    }

    public async Task DeleteLink(long id)
    {
        _logger.LogDebug("Attempting to delete link with id: {Id}", id);

        var link = await _linkRepository.GetByIdAsync(id);
        if (link is null)
        {
            _logger.LogWarning("Delete failed: No link found with id {Id}.", id);
            throw new KeyNotFoundException($"No link found with id '{id}'.");
        }

        _linkRepository.Delete(link);
        await _linkRepository.SaveChangesAsync();

        _logger.LogInformation("Link deleted successfully with id: {Id}.", id);
    }

    public async Task<List<LinkResponse>> GetAllLinks()
    {
        _logger.LogDebug("Retrieving all links from the database ..");
        var links = await _linkRepository.GetAllAsync();

        _logger.LogInformation("Retrieved {Count} links from the database.", links.Count);
        return links.Select(LinkResponse.From).ToList();
    }

    public async Task<List<LinkResponse>> GetLinksByUserId(long userId)
    {
        _logger.LogDebug("Retrieving links for userId: {UserId}", userId);
        var links = await _linkRepository.GetByUserIdAsync(userId);

        _logger.LogInformation("Retrieved {Count} links for userId: {UserId}.", links.Count, userId);
        return links.Select(LinkResponse.From).ToList();
    }
}