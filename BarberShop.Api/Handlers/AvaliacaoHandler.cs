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
            try
            {
                var cliente = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == request.UserId);

                if (cliente == null)
                    return new Response<AvaliacaoResponse?>(null, 404, "Cliente não encontrado");

                if (request.Estrelas < 1 || request.Estrelas > 5)
                    return new Response<AvaliacaoResponse?>(null, 400, "A nota deve ser entre 1 e 5");

                var avaliacao = new Avaliacao
                {
                    UserId = request.UserId,
                    AgendamentoId = request.AgendamentoId,
                    Estrelas = request.Estrelas,
                    Comentario = request.Comentario,
                    Data = DateTime.UtcNow
                };

                await _context.Avaliacoes.AddAsync(avaliacao);
                await _context.SaveChangesAsync();

                var responseData = new AvaliacaoResponse(
                  avaliacao.Id,
                  avaliacao.UserId,
                  avaliacao.AgendamentoId,
                  avaliacao.Estrelas,
                  avaliacao.Comentario,
                  avaliacao.Data,
                  avaliacao.NomeCliente
              );
                return new Response<AvaliacaoResponse?>(responseData, 201, "Avaliação registrada com sucesso!");
            }
            catch (Exception ex)
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

                var cliente = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.UserId);

                if (cliente is null)
                    return new Response<AvaliacaoResponse?>(null, 404, "Cliente não encontrado");

                avaliacao.UserId = request.UserId;
                avaliacao.Estrelas = request.Estrelas;
                avaliacao.Comentario = request.Comentario;

                avaliacao.Data = DateTime.UtcNow;

                _context.Avaliacoes.Update(avaliacao);
                await _context.SaveChangesAsync();

                var response = new AvaliacaoResponse(
                    avaliacao.Id,
                    avaliacao.UserId,
                    avaliacao.AgendamentoId,
                    avaliacao.Estrelas,
                    avaliacao.Comentario,
                    avaliacao.Data,
                    avaliacao.NomeCliente
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
                    avaliacao.UserId,
                    avaliacao.AgendamentoId,
                    avaliacao.Estrelas,
                    avaliacao.Comentario,
                    avaliacao.Data,
                    avaliacao.NomeCliente
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

        public async Task<PagedResponse<List<AvaliacaoResponse>>> GetAllAsync(GetAllAvaliacaoRequest request)
        {
            try
            {
                var query = _context
                   .Avaliacoes
                   .AsNoTracking()
                   //.Include(x => x.UserId)
                   // .Include(x => x.Comentario)
                   .Where(x => x.UserId == request.UserId)
                   .OrderBy(x => x.Id);

                var avaliacoes = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var responses = new List<AvaliacaoResponse>();
                foreach (var av in avaliacoes)
                {
                    var user = await _context.Users.FindAsync(av.UserId);
                    responses.Add(new AvaliacaoResponse(
                        av.Id,
                        av.UserId,
                        av.AgendamentoId,
                        av.Estrelas,
                        av.Comentario,
                        av.Data,
                        user?.NomeCompleto ?? "Desconhecido"
                    ));
                }

                var count = await query.CountAsync();

                return new PagedResponse<List<AvaliacaoResponse>>(responses,
                    count,
                    request.PageNumber,
                    request.PageSize);
            }
            catch (Exception ex)
            {
                return new PagedResponse<List<AvaliacaoResponse>>(null, 500, "Erro: " + ex.Message);
            }
        }

        public async Task<PagedResponse<List<AvaliacaoResponse>>> GetAllPublicAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _context
                    .Avaliacoes
                    .AsNoTracking()
                    .OrderByDescending(x => x.Data);

                var avaliacoes = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responses = new List<AvaliacaoResponse>();
                foreach (var av in avaliacoes)
                {
                    var user = await _context.Users.FindAsync(av.UserId);
                    responses.Add(new AvaliacaoResponse(
                        av.Id,
                        av.UserId,
                        av.AgendamentoId,
                        av.Estrelas,
                        av.Comentario,
                        av.Data,
                        user?.NomeCompleto ?? "Desconhecido"
                    ));
                }

                var count = await query.CountAsync();
                return new PagedResponse<List<AvaliacaoResponse>>(responses, count, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message} | Inner: {ex.InnerException?.Message}");
                return new PagedResponse<List<AvaliacaoResponse>>(null, 500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
