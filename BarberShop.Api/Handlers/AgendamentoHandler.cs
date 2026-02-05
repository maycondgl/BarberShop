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
        public async Task<Response<AgendamentoResponse>> CreateAsync(CreateAgendamentoRequest request)
        {
            try
            {
                var corte = await _context.Cortes
                    .FirstOrDefaultAsync(x => x.Id == request.CorteId);

                var clienteExiste = await _context.Clientes
                    .AnyAsync(x => x.Id == request.ClienteId);

                if (corte is null || !clienteExiste)
                    return new Response<AgendamentoResponse>(null, 404, "Cliente ou corte não encontrado");

                var agendamento = new Agendamento
                {
                    ClienteId = request.ClienteId,
                    CorteId = request.CorteId,
                    Data = request.Data,
                    ValorPago = corte.Preco,
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
                     agendamento.ValorPago,
                     (int)agendamento.Tempo.TotalMinutes,
                     agendamento.Status.ToString()
                 );

                return new Response<AgendamentoResponse>(response, 201, "Agendamento criado");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Falha ao criar agendamento");
            }
        }

        public Task<Response<Agendamento>> UpdateAsync(UpdateAgendamentoRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Response<Agendamento>> DeleteAsync(DeleteAgendamentoRequest request)
        {
            throw new NotImplementedException();
        }        

        public Task<Response<Agendamento>> GetByIdAsync(GetAgendamentoByIdRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Response<List<Agendamento>>> GetAllAsync(GetAllAgendamentoRequest request)
        {
            throw new NotImplementedException();
        }


    }
}
