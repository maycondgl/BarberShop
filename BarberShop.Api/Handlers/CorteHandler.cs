using BarberShop.Api.Data;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Corte;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Handlers
{
    public class CorteHandler : ICorteHandler
    {
        private readonly BarberShopContext _context;

        public CorteHandler(BarberShopContext context)
        {
            _context = context;
        }
        public async Task<Response<CorteResponse?>> CreateAsync(CreateCorteRequest request)
        {
            try
            {
                var existe = await _context.Cortes
            .AnyAsync(x => x.Titulo == request.Titulo);

                if (existe)
                    return new Response<CorteResponse?>(null, 400, "Já existe um corte com esse título");

                var corte = new Corte
                {
                    Titulo = request.Titulo,
                    Preco = request.Preco,
                    DuracaoMinutos = request.DuracaoMinutos,
                    Role = request.Role
                };

                await _context.Cortes.AddAsync(corte);
                await _context.SaveChangesAsync();

                var response = new CorteResponse(
                    corte.Id,
                    corte.Titulo,
                    corte.Preco,
                    corte.DuracaoMinutos,
                    corte.Role
                    );
                return new Response<CorteResponse?>(response, 201, "Corte criado com sucesso");
            }
            catch
            {
                return new Response<CorteResponse?>(null, 500, "Erro ao criar corte");
            }

        }
        public async Task<Response<CorteResponse?>> UpdateAsync(UpdateCorteRequest request)
        {
            try
            {
                var corte = await _context.Cortes
           .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (corte is null)
                    return new Response<CorteResponse?>(
                        null,
                        404,
                        "Corte não encontrado"
                    );

                corte.Titulo = request.Titulo;
                corte.Preco = request.Preco;
                corte.DuracaoMinutos = request.DuracaoMinutos;
                corte.Role = request.Role;

                await _context.SaveChangesAsync();

                var response = new CorteResponse(
                    corte.Id,
                    corte.Titulo,
                    corte.Preco,
                    corte.DuracaoMinutos,
                    corte.Role);

                return new Response<CorteResponse?>(response, 200, "Corte atualizado com sucesso");
            }
            catch
            {
                return new Response<CorteResponse?>(null, 500, "Erro ao atualizar o corte");

            }
        }
        public async Task<Response<CorteResponse?>> DeleteAsync(long id)
        {
            try
            {
                var corte = await _context
                 .Cortes
                 .FirstOrDefaultAsync(x => x.Id == id);

                if (corte is null)
                    return new Response<CorteResponse?>(null, 404, "Corte não encontrado");

                var response = new CorteResponse(
                        corte.Id,
                        corte.Titulo,
                        corte.Preco,
                        corte.DuracaoMinutos,
                        corte.Role
                    );

                _context.Cortes.Remove(corte);
                await _context.SaveChangesAsync();

                return new Response<CorteResponse?>(response, message: "Corte excluído com sucesso");
            }
            catch
            {
                return new Response<CorteResponse?>(null, 500, "Erro ao deletar o corte");

            }
        }
         public async Task<Response<Corte?>> GetByIdAsync(GetCorteByIdRequest request)
         {
            try
            {
                var corte = await _context
                 .Cortes
                 .AsNoTracking()
                 .FirstOrDefaultAsync(x => x.Id == request.Id);

                return corte is null
                    ? new Response<Corte?>(null, 404, "Corte não encontrado")
                    : new Response<Corte?>(corte);
            }
            catch
            {
                return new Response<Corte?>(null, 500, "Erro ao recuperar um corte");

            }
        }

        public async Task<PagedResponse<List<Corte>>> GetAllAsync(GetAllCorteRequest request)
        {
            try
            {
                var query = _context
                   .Cortes
                   .AsNoTracking()
                   .OrderBy(x => x.Id);

                var cortes = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var count = await query.CountAsync();

                return new PagedResponse<List<Corte>>(cortes,
                    count,
                    request.PageNumber,
                    request.PageSize);
            }
            catch
            {
                return new PagedResponse<List<Corte>>(null, 500, "Falha ao recuperar todos os cortes");

            }
        }     
    }
}
