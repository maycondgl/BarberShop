using BarberShop.Api.common.Api;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;
using Microsoft.AspNetCore.Mvc;

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

    private static async Task<IResult> HandleAsync(HttpRequest request)
    {
        try
        {
            var form = await request.ReadFormAsync();
            var file = form.Files["file"];

            if (file is null || file.Length == 0)
                return Results.BadRequest("Imagem inválida");

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";

            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Imgs",
                "cortes");

            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return Results.Ok(new UploadCorteImagemResponse
            {
                ImagemUrl = $"http://localhost:5131/Imgs/cortes/{fileName}"
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}