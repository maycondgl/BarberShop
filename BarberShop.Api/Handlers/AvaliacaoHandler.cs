using BarberShop.Api.Data;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Avaliacao;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Handlers
{
    public class AvaliacaoHandler : IAvaliacaoHandler
    {
        private readonly BarberShopContext _context;

        public AvaliacaoHandler(BarberShopContext context)
        {
            _context = context;
        }
        public async Task<Response<AvaliacaoResponse?>> CreateAsync(CreateAvaliacaoRequest request)
        {
            try{
                var cliente = await _context.Clientes
                .FirstOrDefaultAsync(x => x.Id == request.ClienteId);

                if (cliente == null)
                    return new Response<AvaliacaoResponse?>(null, 404, "Cliente não encontrado");

                if (request.Estrelas < 1 || request.Estrelas > 5)
                    return new Response<AvaliacaoResponse?>(null, 400, "A nota deve ser entre 1 e 5");

                var avaliacao = new Avaliacao
                {
                    ClienteId = request.ClienteId,
                    Estrelas = request.Estrelas,
                    Comentario = request.Comentario,
                    Data = DateTime.UtcNow,
                    UserId = "sistema"
                };

                await _context.Avaliacoes.AddAsync(avaliacao);
                await _context.SaveChangesAsync();

                var responseData = new AvaliacaoResponse(
                  avaliacao.Id,
                  avaliacao.ClienteId,
                  avaliacao.Estrelas,
                  avaliacao.Comentario,
                  avaliacao.Data
              );
                return new Response<AvaliacaoResponse?>(responseData, 201, "Avaliação registrada com sucesso!");
            }
            catch(Exception ex)
            {
                return new Response<AvaliacaoResponse?>(null, 400, ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<Response<AvaliacaoResponse?>> UpdateAsync(UpdateAvaliacaoRequest request)
        {
            try
            {
                var avaliacao = await _context
            .Avaliacoes
            .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (avaliacao is null)
                    return new Response<AvaliacaoResponse?>(null, 404, "Avaliação não encontrado");

                var cliente = await _context.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ClienteId);

                if (cliente is null)
                    return new Response<AvaliacaoResponse?>(null, 404, "Cliente não encontrado");

                avaliacao.ClienteId = request.ClienteId;
                avaliacao.Estrelas = request.Estrelas;
                avaliacao.Comentario = request.Comentario;

                avaliacao.Data = DateTime.UtcNow;
                avaliacao.UserId = "sistema";

                _context.Avaliacoes.Update(avaliacao);
                await _context.SaveChangesAsync();

                var response = new AvaliacaoResponse(
                    avaliacao.Id,
                    avaliacao.ClienteId,
                    avaliacao.Estrelas,
                    avaliacao.Comentario,
                    avaliacao.Data
                  );

                return new Response<AvaliacaoResponse?>(response, 200, "Avaliação atualizado");
            }
            catch
            {
                return new Response<AvaliacaoResponse?>(null, 500, "Falha ao atualizar avaliação");
            }
        }

        public async Task<Response<AvaliacaoResponse?>> DeleteAsync(long id)
        {
            try
            {
                var avaliacao = await _context
                   .Avaliacoes
                   .FirstOrDefaultAsync(x => x.Id == id);

                if (avaliacao is null)
                    return new Response<AvaliacaoResponse?>(null, 404, "Avaliação não encontrada");

                var response = new AvaliacaoResponse(
                    avaliacao.Id,
                    avaliacao.ClienteId,
                    avaliacao.Estrelas,
                    avaliacao.Comentario,
                    avaliacao.Data
                );

                _context.Avaliacoes.Remove(avaliacao);
                await _context.SaveChangesAsync();

                return new Response<AvaliacaoResponse?>(response, message: "Avaliação excluída com sucesso");
            }
            catch
            {
                return new Response<AvaliacaoResponse?>(null, 500, "Não foi possível excluir a avaliação");
            }
        }
        
        public async Task<Response<Avaliacao?>> GetByIdAsync(GetAvaliacaoByIdRequest request)
        {
            try
            {
                var avaliacao = await _context
                    .Avaliacoes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                return avaliacao is null
                    ? new Response<Avaliacao?>(null, 404, "Avaliação não encontrada")
                    : new Response<Avaliacao?>(avaliacao);
            }
            catch
            {
                return new Response<Avaliacao?>(null, 500, "Não foi possível recuperar avaliação");
            }
        }

        public async Task<PagedResponse<List<Avaliacao>>> GetAllAsync(GetAllAvaliacaoRequest request)
        {
            try
            {
                var query = _context
                   .Avaliacoes
                   .AsNoTracking()
                    .Include(x => x.Cliente)
                   // .Include(x => x.Comentario)
                   // .Where(x => x.UserId == request.UserId)
                   .OrderBy(x => x.Id);

                var avaliacoes = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var count = await query.CountAsync();

                return new PagedResponse<List<Avaliacao>>(avaliacoes,
                    count,
                    request.PageNumber,
                    request.PageSize);
            }
            catch(Exception ex)
            {
                return new PagedResponse<List<Avaliacao>>(null, 500, "Erro: " + ex.Message);
            }
        }
    }        
}
