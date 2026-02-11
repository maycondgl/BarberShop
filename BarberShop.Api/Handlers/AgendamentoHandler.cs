using BarberShop.Api.Data;
using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamentos;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Handlers
{
    public class AgendamentoHandler : IAgendamentoHandler
    {
        private readonly BarberShopContext _context;

        public AgendamentoHandler(BarberShopContext context)
        {
            _context = context;
        }
        public async Task<Response<AgendamentoResponse?>> CreateAsync(CreateAgendamentoRequest request)
        {
            try
            {
                var corte = await _context.Cortes
                    .FirstOrDefaultAsync(x => x.Id == request.CorteId);

                var clienteExiste = await _context.Clientes
                    .AnyAsync(x => x.Id == request.ClienteId);

                if (corte is null || !clienteExiste)
                    return new Response<AgendamentoResponse?>(null, 404, "Cliente ou corte não encontrado");

                var agendamento = new Agendamento
                {
                    ClienteId = request.ClienteId,
                    CorteId = request.CorteId,
                    Data = request.Data,
                    Valor = corte.Preco,
                    Tempo = TimeSpan.FromMinutes(corte.DuracaoMinutos),
                    Status = EStatusAgendamento.Pendente,
                    UserId = "sistema"
                };

                _context.Agendamentos.Add(agendamento);
                await _context.SaveChangesAsync();

                var response = new AgendamentoResponse(
                     agendamento.Id,
                     agendamento.ClienteId,
                     agendamento.CorteId,
                     agendamento.Data,
                     agendamento.Valor,
                     (int)agendamento.Tempo.TotalMinutes,
                     agendamento.Status.ToString()
                 );

                return new Response<AgendamentoResponse?>(response, 201, "Agendamento criado");
            }
            catch
            {               
               return new Response<AgendamentoResponse?>(null, 500, "Falha ao criar agendamento");
            }
        }

        public async Task<Response<AgendamentoResponse?>> UpdateAsync(UpdateAgendamentoRequest request)
        {
            try
            {
                var agendamento = await _context
                    .Agendamentos
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (agendamento is null)
                    return new Response<AgendamentoResponse?>(null, 404, "Agendamento não encontrado");

                var corte = await _context.Cortes
                        .FirstOrDefaultAsync(x => x.Id == request.CorteId);

                var clienteExiste = await _context.Clientes
                    .AnyAsync(x => x.Id == request.ClienteId);

                if (corte is null || !clienteExiste)
                    return new Response<AgendamentoResponse?>(null, 404, "Cliente ou corte não encontrado");

                agendamento.ClienteId = request.ClienteId;
                agendamento.CorteId = request.CorteId;
                agendamento.Data = request.Data;

                agendamento.Valor = corte.Preco;
                agendamento.Tempo = TimeSpan.FromMinutes(corte.DuracaoMinutos);

                agendamento.Status = EStatusAgendamento.Pendente;
                agendamento.UserId = "sistema";

                _context.Agendamentos.Update(agendamento);
                await _context.SaveChangesAsync();

                var response = new AgendamentoResponse(
                    agendamento.Id,
                    agendamento.ClienteId,
                    agendamento.CorteId,
                    agendamento.Data,
                    agendamento.Valor,
                    (int)agendamento.Tempo.TotalMinutes,
                    agendamento.Status.ToString()
                  );

                return new Response<AgendamentoResponse?>(response, 200, "Agendamento atualizado");
            }
            catch
            {
                return new Response<AgendamentoResponse?>(null, 500, "Não foi possível alterar o agendamento");
            }
        }

        public async Task<Response<AgendamentoResponse?>> DeleteAsync(long id)
        {
            try
            {
                var agendamento = await _context
                    .Agendamentos
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (agendamento is null)
                    return new Response<AgendamentoResponse?>(null, 404, "Agendamento não encontrado");

                var response = new AgendamentoResponse(
                    agendamento.Id,
                    agendamento.ClienteId,
                    agendamento.CorteId,
                    agendamento.Data,
                    agendamento.Valor,
                    (int)agendamento.Tempo.TotalMinutes,
                    agendamento.Status.ToString()
                );

                _context.Agendamentos.Remove(agendamento);
                await _context.SaveChangesAsync();

                return new Response<AgendamentoResponse?>(response, message: "Agendamento excluído com sucesso");
            }
            catch
            {
                return new Response<AgendamentoResponse?>(null, 500, "Não foi possível excluir o agendamento");
            }
        }

        public async Task<Response<Agendamento?>> GetByIdAsync(GetAgendamentoByIdRequest request)
        {
            try
            {
                var agendamento = await _context
                    .Agendamentos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                return agendamento is null
                    ? new Response<Agendamento?>(null, 404, "Agendamento não encontrado")
                    : new Response<Agendamento?>(agendamento);
            }
            catch
            {
                return new Response<Agendamento?>(null, 500, "Não foi possível recuperar agendamento");
            }
        }

        public async Task<PagedResponse<List<Agendamento>>> GetAllAsync(GetAllAgendamentoRequest request)
        {
            try
            {
                var query = _context
                    .Agendamentos
                    .AsNoTracking()
                     .Include(x => x.Cliente)
                    .Include(x => x.Corte)
                    // .Where(x => x.UserId == request.UserId)
                    .OrderBy(x => x.ClienteId);

                var agendamentos = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var count = await query.CountAsync();

                return new PagedResponse<List<Agendamento>>(agendamentos,
                    count,
                    request.PageNumber,
                    request.PageSize);
            }
            catch
            {
                return new PagedResponse<List<Agendamento>>(null, 500, "Não foi possível consultar os agendamentos");
            }
        }


    }
}
