using System.Xml.Serialization;
using Shortly.Application.Interfaces;

namespace Shortly.Endpoints;

public static class LinkApiEndpoint
{
    public static void MapLinkApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // POST /api/urls — Crear URL acortada
        api.MapPost("/urls", async (HttpContext ctx, ILinkService linkService) =>
        {
            CreateLinkRequest? req;

            try
            {
                req = await ParseCreateLinkRequest(ctx);
            }
            catch (Exception)
            {
                return RespondWith(ctx, new ErrorResponse("Invalid request body format."), 400);
            }

            if (req is null || string.IsNullOrWhiteSpace(req.Url))
                return RespondWith(ctx, new ErrorResponse("URL is required."), 400);

            if (!Uri.TryCreate(req.Url, UriKind.Absolute, out var parsedUrl) ||
                (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps))
            {
                return RespondWith(ctx, new ErrorResponse("URL must be a valid absolute http/https URL."), 400);
            }

            try
            {
                // Usamos userId=1 (admin) por ahora, ya que no hay auth en la API
                var link = await linkService.CreateLink(req.Url, 1);
                return RespondWith(ctx, link, 201, location: $"/api/urls/{link.Id}");
            }
            catch (ArgumentException ex)
            {
                return RespondWith(ctx, new ErrorResponse(ex.Message), 400);
            }
        })
        .WithName("CreateUrl")
        .WithSummary("Create a shortened URL")
        .Produces(201)
        .Produces(400);

        // GET /api/urls — Listar todas las URLs
        api.MapGet("/urls", async (HttpContext ctx, ILinkService linkService) =>
        {
            var links = await linkService.GetAllLinks();
            return RespondWith(ctx, links, 200);
        })
        .WithName("GetAllUrls")
        .WithSummary("Get all shortened URLs")
        .Produces(200)
        .Produces(406);

        // GET /api/urls/{id} — Obtener una URL por ID
        api.MapGet("/urls/{id:long}", async (long id, HttpContext ctx, ILinkService linkService) =>
        {
            try
            {
                var link = await linkService.GetLinkById(id);
                return RespondWith(ctx, link, 200);
            }
            catch (KeyNotFoundException ex)
            {
                return RespondWith(ctx, new ErrorResponse(ex.Message), 404);
            }
        })
        .WithName("GetUrlById")
        .WithSummary("Get a shortened URL by ID")
        .Produces(200)
        .Produces(404)
        .Produces(406);

        // DELETE /api/urls/{id} — Eliminar una URL
        api.MapDelete("/urls/{id:long}", async (long id, HttpContext ctx, ILinkService linkService) =>
        {
            try
            {
                await linkService.DeleteLink(id);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return RespondWith(ctx, new ErrorResponse(ex.Message), 404);
            }
        })
        .WithName("DeleteUrl")
        .WithSummary("Delete a shortened URL")
        .Produces(204)
        .Produces(404);

        // GET /api/stats — Estadísticas de uso
        api.MapGet("/stats", async (HttpContext ctx, ILinkService linkService) =>
        {
            var links = await linkService.GetAllLinks();
            var stats = new StatsResponse
            {
                TotalLinks = links.Count,
                TotalClicks = links.Sum(l => l.Clicks),
                TopLinks = links.OrderByDescending(l => l.Clicks).Take(5).ToList()
            };
            return RespondWith(ctx, stats, 200);
        })
        .WithName("GetStats")
        .WithSummary("Get usage statistics")
        .Produces(200)
        .Produces(406);
    }

    // Lee el body del POST y lo convierte en CreateLinkRequest.
    // Si el Content-Type dice XML, lo leemos como XML, si no, como JSON.
    private static async Task<CreateLinkRequest?> ParseCreateLinkRequest(HttpContext ctx)
    {
        var contentType = ctx.Request.ContentType ?? "";

        if (contentType.Contains("xml"))
        {
            var body = await new StreamReader(ctx.Request.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
                return null;

            var serializer = new XmlSerializer(typeof(CreateLinkRequest));
            using var reader = new StringReader(body);
            return serializer.Deserialize(reader) as CreateLinkRequest;
        }

        return await ctx.Request.ReadFromJsonAsync<CreateLinkRequest>();
    }

    // Decide si responder en JSON o XML según el header Accept, con status code y Location opcionales
    private static IResult RespondWith<T>(HttpContext ctx, T data, int statusCode, string? location = null)
    {
        var accept = ctx.Request.Headers.Accept.ToString();

        if (!string.IsNullOrEmpty(location))
            ctx.Response.Headers.Location = location;

        // JSON primero: si el Accept incluye json (o está vacío/*/*), respondemos JSON.
        if (string.IsNullOrEmpty(accept) || accept.Contains("*/*") || accept.Contains("application/json"))
        {
            return Results.Json(data, statusCode: statusCode);
        }

        if (accept.Contains("application/xml") || accept.Contains("text/xml"))
        {
            var serializer = new XmlSerializer(typeof(T));
            var writer = new StringWriter();
            serializer.Serialize(writer, data);
            return Results.Content(writer.ToString(), "application/xml", statusCode: statusCode);
        }

        return Results.StatusCode(406);
    }
}

// Request body para POST /api/urls
public class CreateLinkRequest
{
    public string Url { get; set; } = string.Empty;
}

// Response para GET /api/stats
public class StatsResponse
{
    public int TotalLinks { get; set; }
    public int TotalClicks { get; set; }
    public List<Shortly.Application.DTOs.LinkResponse> TopLinks { get; set; } = [];
}

// Response para errores, serializable en JSON y XML
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;

    public ErrorResponse() { }

    public ErrorResponse(string error) => Error = error;
}