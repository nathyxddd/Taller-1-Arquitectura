## Primer uso

**Herramienta utilizada:** Claude
**Prompt utilizado:** "Puedes decirme como deberíamos distribuir las tareas si somos dos estudiantes realizando el taller? Además, quiero que revises una vez más el proyecto y nos digas que ramas debe trabajar cada estudiante para no tener conflicto entre las tareas."
**Respuesta obtenida:** Se propuso la distribución de tareas, se detallaron las ramas y se comentó como diseñar a grandes rasgos las tareas.
**Cómo se integró:** Los estudiantes se organizaron y comenzaron a desarrollar sus tareas en las distintas ramas.

## Segundo uso

**Herramienta utilizada:** Claude
**Prompt utilizado:** “Ayudame a implementar un middleware personalizado en ASP.NET Core que agregue cabeceras de seguridad (HSTS, X-Content-Type-Options, X-Frame-Options, Referrer-Policy, y Permissions-Policy) de manera global a todas las respuestas de la aplicación. ¿Cómo puedo estructurar la clase de middleware y agregarlo en Program.cs?"
**Respuesta obtenida:** Se proporcionó la estructura de una clase de middleware que inyecta las cabeceras directamente en el objeto context.Response.Headers y luego delega el procesamiento al siguiente middleware con await _next(context). Se indicó el orden exacto de registro en Program.cs.
**Cómo se integró:** Se creó el archivo SecurityHeadersMiddleware.cs en la carpeta Infrastructure/Middleware/ y se registró en Program.cs para mitigar ataques como Clickjacking, MIME-sniffing y degradación SSL.

## Tercer uso

**Herramienta utilizada:** Claude 
**Prompt utilizado:** "Tengo una función en C# que genera un código corto a partir de un ULID, pero necesito asegurarme de que no exponga el timestamp de creación del enlace ni sea secuencial. ¿Cómo puedo transformar un ULID con SHA256 y Base62 para garantizar que la función sea segura y no pierda unicidad?"  
**Respuesta obtenida:** Se explicó la forma de aplicar el algoritmo SHA-256 sobre la representación en string del ULID y luego convertir el hash resultante a una representación alfanumérica en Base62, seleccionando los primeros 12 caracteres.  
**Cómo se integró:** Se implementó y verificó el método GenerateSecureShortUrl() en LinkService.cs, asegurando que la función genere identificadores impredecibles de 12 caracteres sin revelar marcas de tiempo.

## Cuarto uso

**Herramienta utilizada:** Claude  
**Prompt utilizado:** "En mi API de ASP.NET Core devuelvo errores en texto plano o con objetos personalizados, pero necesito ajustarme al estándar RFC 7807 (application/problem+json). ¿Cuál es la forma correcta de usar Results.Problem() en Minimal APIs para errores 400 y 404?"  
**Respuesta obtenida:** Se explicó la firma del método Results.Problem(title, detail, statusCode), detallando cómo este método configura automáticamente el Content-Type application/problem+json y estructura los campos legibles por máquina.  
**Cómo se integró:** Se actualizó LinkApiEndpoint.cs y UrlRedirectEndpoint.cs reemplazando los retornos de error simples por invocaciones a Results.Problem().

## Quinto uso

**Herramienta utilizada:** Claude 
**Prompt utilizado:** "Qué comandos curl puedo utilizar en la consola para verificar manualmente la compresión Brotli/Gzip, la respuesta 304 Not Modified enviando If-None-Match y el código de redirección HTTP 307 de mi aplicación?"  
**Respuesta obtenida:** Se entregó una lista de comandos curl incluyendo -H "Accept-Encoding: br", -H "If-None-Match: \"...\"", y -I para inspeccionar cabeceras HTTP y códigos de estado.  
**Cómo se integró:** Se ejecutaron los comandos sugeridos en la terminal local para comprobar el correcto funcionamiento de las políticas de caché, compresión y redirecciones condicionales.
