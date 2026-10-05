using BarberShop.Api.Data;
using BarberShop.Api.Services;
using BarberShop.Core.Enums;
using BarberShop.Core.Extensions;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Handlers
{
    public class AgendamentoHandler : IAgendamentoHandler
    {
        private readonly BarberShopContext _context;
        private readonly IAgendamentoNotificationService _notificationService;

        public AgendamentoHandler(
            BarberShopContext context,
            IAgendamentoNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
        private static bool _columnsChecked = false;
        private async Task EnsureColumnsExistsAsync()
        {
            if (_columnsChecked) return;
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Agendamento' AND COLUMN_NAME = 'DescricaoServicos')
                    BEGIN
                        ALTER TABLE [Agendamento] ADD [DescricaoServicos] NVARCHAR(500) NULL;
                    END
                ");
                _columnsChecked = true;
            }
            catch
            {
                // Silencioso para provedores sem suporte a SQL raw (ex: testes in-memory)
            }
        }

        public async Task<Response<AgendamentoResponse?>> CreateAsync(CreateAgendamentoRequest request)
        {
            try
            {
                await EnsureColumnsExistsAsync();

                var idsCortes = request.CorteIds != null && request.CorteIds.Any()
                    ? request.CorteIds.Distinct().ToList()
                    : new List<long> { request.CorteId };

                var cortes = await _context.Cortes
                    .Where(x => idsCortes.Contains(x.Id))
                    .ToListAsync();

                var cliente = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == request.UserId);

                if (!cortes.Any() || cliente is null)
                    return new Response<AgendamentoResponse?>(null, 404, "Cliente ou corte não encontrado");

                if (request.Data.Date < DateTime.Today)
                    return new Response<AgendamentoResponse?>(null, 400, "Não é possível agendar para uma data passada.");

                if (request.Data.Date > DateTime.Today.AddDays(15))
                    return new Response<AgendamentoResponse?>(null, 400, "Agendamentos só podem ser realizados com até 15 dias de antecedência.");

                if (request.Data.DayOfWeek == DayOfWeek.Sunday)
                    return new Response<AgendamentoResponse?>(null, 400, "A barbearia não funciona aos domingos.");

                bool diaFechado = false;
                try
                {
                    diaFechado = await _context.DiasFechados
                        .AnyAsync(x => x.Data.Date == request.Data.Date);
                }
                catch
                {
                    diaFechado = false;
                }

                if (diaFechado)
                    return new Response<AgendamentoResponse?>(null, 400, "A barbearia estará fechada nesta data.");

                var duracaoMinutos = cortes.Sum(c => c.DuracaoMinutos > 0 ? c.DuracaoMinutos : 40);
                var duracao = TimeSpan.FromMinutes(duracaoMinutos);
                var valorTotal = cortes.Sum(c => c.Preco);
                var tituloCombinado = string.Join(" + ", cortes.Select(c => c.Titulo));
                var horaInicio = request.Data.TimeOfDay;

                if (request.Data.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (horaInicio < new TimeSpan(7, 0, 0) || horaInicio.Add(duracao) > new TimeSpan(12, 0, 0))
                        return new Response<AgendamentoResponse?>(null, 400, "Aos sábados, os agendamentos ocorrem entre 07:00 e 12:00.");
                }
                else
                {
                    if (horaInicio < new TimeSpan(8, 0, 0) || horaInicio.Add(duracao) > new TimeSpan(19, 0, 0))
                        return new Response<AgendamentoResponse?>(null, 400, "De segunda a sexta, os agendamentos ocorrem entre 08:00 e 19:00.");
                }

                var dataInicio = request.Data.Date;
                var dataFim = dataInicio.AddDays(1);

                var agendamentosDoDia = await _context.Agendamentos
                    .Where(a => a.Data >= dataInicio && a.Data < dataFim && a.Status != EStatusAgendamento.Cancelado
                        && (!request.BarbeiroId.HasValue || a.BarbeiroId == null || a.BarbeiroId == request.BarbeiroId.Value))
                    .Select(a => new { a.Data, a.Tempo })
                    .ToListAsync();

                var inicio = request.Data;
                var fim = inicio.Add(duracao);

                bool temConflito = agendamentosDoDia.Any(a =>
                {
                    var aDuracao = a.Tempo > TimeSpan.Zero ? a.Tempo : TimeSpan.FromMinutes(40);
                    var aFim = a.Data.Add(aDuracao);
                    return inicio < aFim && a.Data < fim;
                });

                if (temConflito)
                    return new Response<AgendamentoResponse?>(null, 400, "Ops! Este horário acabou de ser reservado.");

                var filialNome = "";
                if (request.FilialId.HasValue)
                {
                    filialNome = await _context.Filiais
                        .Where(f => f.Id == request.FilialId.Value)
                        .Select(f => f.Nome)
                        .FirstOrDefaultAsync() ?? "";
                }

                var barbeiroNome = "";
                if (request.BarbeiroId.HasValue)
                {
                    barbeiroNome = await _context.Barbeiros
                        .Where(b => b.Id == request.BarbeiroId.Value)
                        .Select(b => b.Nome)
                        .FirstOrDefaultAsync() ?? "";
                }

                var agendamento = new Agendamento
                {
                    UserId = request.UserId,
                    CorteId = cortes.First().Id,
                    FilialId = request.FilialId,
                    BarbeiroId = request.BarbeiroId,
                    Data = DateTime.SpecifyKind(request.Data, DateTimeKind.Unspecified),
                    Valor = valorTotal,
                    Tempo = duracao,
                    Status = EStatusAgendamento.Pendente,
                    DescricaoServicos = tituloCombinado
                };

                _context.Agendamentos.Add(agendamento);
                await _context.SaveChangesAsync();

                var response = new AgendamentoResponse(
                     agendamento.Id,
                     agendamento.UserId,
                     agendamento.CorteId,
                     agendamento.Data,
                     agendamento.Valor,
                     (int)agendamento.Tempo.TotalMinutes,
                     agendamento.Status.ToString(),
                     cliente.NomeCompleto,
                     tituloCombinado,
                     agendamento.FilialId,
                     filialNome,
                     agendamento.BarbeiroId,
                     barbeiroNome
                 );

                await _notificationService.NotifyNovoAgendamentoAsync(response);

                return new Response<AgendamentoResponse?>(response, 201, "Agendamento criado");
            }
            catch (Exception ex)
            {               
               return new Response<AgendamentoResponse?>(null, 500, "Falha ao criar agendamento: " + ex.Message);
            }
        }

        public async Task<Response<AgendamentoResponse?>> UpdateAsync(UpdateAgendamentoRequest request)
        {
            try
            {
                await EnsureColumnsExistsAsync();

                var agendamento = await _context
                    .Agendamentos
                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

                if (agendamento is null)
                    return new Response<AgendamentoResponse?>(null, 404, "Agendamento não encontrado");

                var idsCortes = request.CorteIds != null && request.CorteIds.Any()
                    ? request.CorteIds.Distinct().ToList()
                    : new List<long> { request.CorteId };

                var cortes = await _context.Cortes
                    .Where(x => idsCortes.Contains(x.Id))
                    .ToListAsync();

                if (!cortes.Any())
                    return new Response<AgendamentoResponse?>(null, 404, "Corte não encontrado");

                var duracaoMinutos = cortes.Sum(c => c.DuracaoMinutos > 0 ? c.DuracaoMinutos : 40);
                var duracao = TimeSpan.FromMinutes(duracaoMinutos);
                var valorTotal = cortes.Sum(c => c.Preco);
                var tituloCombinado = string.Join(" + ", cortes.Select(c => c.Titulo));

                agendamento.CorteId = cortes.First().Id;
                agendamento.FilialId = request.FilialId;
                agendamento.BarbeiroId = request.BarbeiroId;
                agendamento.Data = request.Data;

                agendamento.Valor = valorTotal;
                agendamento.Tempo = duracao;
                agendamento.DescricaoServicos = tituloCombinado;

                agendamento.Status = EStatusAgendamento.Pendente;

                _context.Agendamentos.Update(agendamento);
                await _context.SaveChangesAsync();

                var filialNome = "";
                if (agendamento.FilialId.HasValue)
                {
                    filialNome = await _context.Filiais
                        .Where(f => f.Id == agendamento.FilialId.Value)
                        .Select(f => f.Nome)
                        .FirstOrDefaultAsync() ?? "";
                }

                var barbeiroNome = "";
                if (agendamento.BarbeiroId.HasValue)
                {
                    barbeiroNome = await _context.Barbeiros
                        .Where(b => b.Id == agendamento.BarbeiroId.Value)
                        .Select(b => b.Nome)
                        .FirstOrDefaultAsync() ?? "";
                }

                var response = new AgendamentoResponse(
                    agendamento.Id,
                    agendamento.UserId,
                    agendamento.CorteId,
                    agendamento.Data,
                    agendamento.Valor,
                    (int)agendamento.Tempo.TotalMinutes,
                    agendamento.Status.ToString(),
                    agendamento.NomeCliente,
                    tituloCombinado,
                    agendamento.FilialId,
                    filialNome,
                    agendamento.BarbeiroId,
                    barbeiroNome
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
                    .Include(x => x.Corte)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (agendamento is null)
                    return new Response<AgendamentoResponse?>(null, 404, "Agendamento não encontrado");

                var response = new AgendamentoResponse(
                    agendamento.Id,
                    agendamento.UserId,
                    agendamento.CorteId,
                    agendamento.Data,
                    agendamento.Valor,
                    (int)agendamento.Tempo.TotalMinutes,
                    agendamento.Status.ToString(),
                    agendamento.NomeCliente,
                    !string.IsNullOrWhiteSpace(agendamento.DescricaoServicos) ? agendamento.DescricaoServicos : (agendamento.Corte?.Titulo ?? "Sem corte")
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
                    .Include(x => x.Corte)
                    .Include(x => x.Filial)
                    .Include(x => x.Barbeiro)
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
                    .Include(x => x.Corte)
                    .Include(x => x.Filial)
                    .Include(x => x.Barbeiro)
                    .Where(x => x.UserId == request.UserId)
                    .OrderByDescending(x => x.Data);

                var agendamentos = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                foreach (var a in agendamentos)
                {
                    var user = await _context.Users.FindAsync(a.UserId);
                    a.NomeCliente = user?.NomeCompleto ?? "Desconhecido";
                }

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

        public async Task<PagedResponse<List<AgendamentoResponse>?>> GetByPeriodAsync(
            GetAgendamentoByPeriodRequest request)
        {
            var startDate = request.StartDate ?? DateTime.Now.GetFirstDayOfMonth();
            var endDate = request.EndDate ?? DateTime.Now.GetLastDayOfMonth();

            if (endDate.TimeOfDay == TimeSpan.Zero)
            {
                endDate = endDate.Date.AddDays(1).AddTicks(-1);
            }

            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified);

            var query = _context.Agendamentos
                .AsNoTracking()
                .Include(a => a.Corte)
                .Include(a => a.Filial)
                .Include(a => a.Barbeiro)
                .Where(a => a.Data >= startDate && a.Data <= endDate)
                .OrderBy(a => a.Data);

            var count = await query.CountAsync();

            var agendamentos = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<List<AgendamentoResponse>?>(
                agendamentos.Select(a => (AgendamentoResponse)a).ToList(),
                count, request.PageNumber, request.PageSize);
        }

        public async Task<PagedResponse<List<AgendamentoResponse>>> GetAllAdminAsync(GetAllAgendamentoRequest request)
        {
            try
            {
                var query = _context
                    .Agendamentos
                    .AsNoTracking()
                    .Include(x => x.Corte)
                    .Include(x => x.Filial)
                    .Include(x => x.Barbeiro)
                    .OrderByDescending(x => x.Data);

                var agendamentos = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var responses = new List<AgendamentoResponse>();
                foreach (var a in agendamentos)
                {
                    var corteTitulo = !string.IsNullOrWhiteSpace(a.DescricaoServicos)
                        ? a.DescricaoServicos
                        : (await _context.Cortes
                            .Where(c => c.Id == a.CorteId)
                            .Select(c => c.Titulo)
                            .FirstOrDefaultAsync() ?? "Sem corte");

                    var user = await _context.Users.FindAsync(a.UserId);
                    responses.Add(new AgendamentoResponse(
                        a.Id,
                        a.UserId,
                        a.CorteId,
                        a.Data,
                        a.Valor,
                        (int)a.Tempo.TotalMinutes,
                        a.Status.ToString(),
                        user?.NomeCompleto ?? "Desconhecido",
                        corteTitulo,
                        a.FilialId,
                        a.Filial?.Nome ?? "",
                        a.BarbeiroId,
                        a.Barbeiro?.Nome ?? ""
                    ));
                }

                var count = await query.CountAsync();

                return new PagedResponse<List<AgendamentoResponse>>(responses, count, request.PageNumber, request.PageSize);
            }
            catch
            {
                return new PagedResponse<List<AgendamentoResponse>>(null, 500, "Não foi possível consultar os agendamentos");
            }
        }

        public async Task<Response<AgendamentoResponse?>> UpdateStatusAsync(UpdateStatusAgendamentoRequest request)
        {
            try
            {
                var agendamento = await _context.Agendamentos
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (agendamento is null)
                    return new Response<AgendamentoResponse?>(null, 404, "Agendamento não encontrado");

                agendamento.Status = request.Status;
                _context.Agendamentos.Update(agendamento);
                await _context.SaveChangesAsync();

                return new Response<AgendamentoResponse?>(null, 200, "Status atualizado com sucesso");
            }
            catch
            {
                return new Response<AgendamentoResponse?>(null, 500, "Erro ao atualizar status");
            }
        }

    }
}
