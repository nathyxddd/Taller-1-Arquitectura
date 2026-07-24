using System.Security.Cryptography;
using System.Text;
using Shortly.Application.Interfaces;

namespace Shortly.Endpoints;

public static class UrlRedirectEndpoint
{
    public static void MapUrlRedirect(this WebApplication app)
    {
        app.MapGet("/{shortUrl}", async (string shortUrl, HttpContext httpContext, ILinkService linkService) =>
        {
            try
            {
                var link = await linkService.GetLink(shortUrl);

                
                // Generación de ETag estable basado en el estado del enlace (ID y URL de destino)
                var rawEtagData = $"{link.Id}:{link.Url}";
                var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawEtagData));
                var etag = $"\"{Convert.ToHexString(hashBytes)[..16].ToLowerInvariant()}\"";

                var requestEtag = httpContext.Request.Headers.IfNoneMatch.ToString();

                // Verificación de cabeceras condicionales de cliente (If-None-Match)
                if (!string.IsNullOrEmpty(requestEtag) && requestEtag == etag)
                {
                    /* 
                     * Documentación - Ahorro de Ancho de Banda con 304 Not Modified:
                     * El código de estado 304 Not Modified permite ahorrar ancho de banda y reducir la carga de procesamiento
                     * del servidor al responder sin cuerpo de mensaje ni ejecutar redirecciones redundantes. Le comunica al 
                     * cliente (navegador o proxy) que la versión en caché sigue siendo válida y puede ser reutilizada directamente.
                     */
                    return Results.StatusCode(StatusCodes.Status304NotModified);
                }

                // Incrementar contador de accesos en el sistema tras superar validación de caché
                await linkService.IncrementClicks(link.Id);

                // Configuración de cabeceras HTTP de caché en la respuesta
                httpContext.Response.Headers.CacheControl = "private, max-age=3600";
                httpContext.Response.Headers.ETag = etag;
                httpContext.Response.Headers.LastModified = DateTime.UtcNow.ToString("r");
                /*
                 * Documentación - Diferencias Semánticas entre Códigos HTTP de Redirección:
                 * - 301 Moved Permanently: Indica que el recurso ha cambiado de ubicación de manera definitiva. Los clientes y
                 *   buscadores almacenan en caché el nuevo destino permanentemente y pueden reescriturar el método HTTP (ej. POST a GET).
                 * - 302 Found: Redirección temporal clásica. No es persistente en caché y los navegadores históricamente cambian el método a GET.
                 * - 307 Temporary Redirect: Redirección temporal estricta. Causa que el cliente vuelva a realizar la petición al nuevo URI
                 *   CONSERVANDO obligatoriamente el mismo método HTTP y cuerpo original (ej. POST re-envía POST).
                 * - 308 Permanent Redirect: Redirección permanente estricta. Combina la permanencia del 301 con la garantía del 307 de
                 *   reutilizar el mismo método HTTP sin alterations.
                 */
                if (link.Clicks > 100)
                {
                    // Enlaces estables e hiper-visitados (>100 clics): 301 Moved Permanently
                    return Results.Redirect(link.Url, permanent: true, preserveMethod: false);
                }

                // Enlaces nuevos o de menor tráfico (<=100 clics): 307 Temporary Redirect
                return Results.Redirect(link.Url, permanent: false, preserveMethod: true);
            }
            catch (KeyNotFoundException)
            {
                /*
                 * Documentación - Interoperabilidad con RFC 7807:
                 * Problem Details (application/problem+json) estandariza el formato de errores en APIs RESTful,
                 * permitiendo que cualquier cliente consuma respuestas de error estructuradas con machine-readable fields (title, detail, status)
                 * sin depender de implementaciones o contratos propietarios.
                 */
                return Results.Problem(
                    title: "Link Not Found",
                    detail: $"No active link was found for the short URL '{shortUrl}'.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }
        });
    }
}
