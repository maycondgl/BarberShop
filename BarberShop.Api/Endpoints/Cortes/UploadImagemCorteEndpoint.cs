using BarberShop.Api.common.Api;
using BarberShop.Core;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;

namespace BarberShop.Api.Endpoints.Cortes;

public class UploadImagemCorteEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/upload-imagem", HandleAsync)
              .WithName("Cortes: Upload Imagem")
              .WithSummary("Faz upload da imagem do corte")
              .WithOrder(6)
              .Produces<Response<CorteResponse?>>(200)
              .Produces<Response<CorteResponse?>>(404)
              .Produces<Response<CorteResponse?>>(500);

    private static async Task<IResult> HandleAsync(
        HttpRequest request,
        IWebHostEnvironment environment)
    {
        try
        {
            var form = await request.ReadFormAsync();
            var file = form.Files["file"];

            if (file is null || file.Length == 0)
                return Results.BadRequest("Imagem inválida");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Results.BadRequest("Formato de imagem inválido");

            var fileName = $"{Guid.NewGuid()}{extension}";

            var webRootPath = environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var folder = Path.Combine(webRootPath, "Imgs", "cortes");

            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            var backendUrl = Configuration.BackendUrl.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                backendUrl = $"{request.Scheme}://{request.Host}";
            }

            return Results.Ok(new UploadCorteImagemResponse
            {
                ImagemUrl = $"{backendUrl}/Imgs/cortes/{fileName}"
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}