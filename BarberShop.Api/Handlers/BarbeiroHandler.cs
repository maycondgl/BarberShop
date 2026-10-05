using BarberShop.Api.Data;
using BarberShop.Api.Models;
using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Agendamento;
using BarberShop.Core.Responses.Avaliacao;
using BarberShop.Core.Responses.Barbeiro;
using BarberShop.Core.Responses.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BarberShop.Api.Handlers
{
    public class BarbeiroHandler : IBarbeiroHandler
    {
        private static readonly CultureInfo PtBr = new("pt-BR");
        private readonly BarberShopContext _context;
        private readonly UserManager<User>? _userManager;
        private readonly RoleManager<IdentityRole<long>>? _roleManager;
        private static bool _tableChecked = false;

        public BarbeiroHandler(
            BarberShopContext context,
            UserManager<User>? userManager = null,
            RoleManager<IdentityRole<long>>? roleManager = null)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private async Task EnsureTableExistsAsync()
        {
            if (_tableChecked) return;
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Barbeiro')
                    BEGIN
                        CREATE TABLE [Barbeiro] (
                            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Nome] NVARCHAR(100) NOT NULL,
                            [FotoUrl] NVARCHAR(500) NULL,
                            [FilialId] BIGINT NULL,
                            [Ativo] BIT NOT NULL DEFAULT 1,
                            [UsuarioId] BIGINT NULL
                        );
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Barbeiro' AND COLUMN_NAME = 'UsuarioId')
                    BEGIN
                        ALTER TABLE [Barbeiro] ADD [UsuarioId] BIGINT NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Agendamento' AND COLUMN_NAME = 'BarbeiroId')
                    BEGIN
                        ALTER TABLE [Agendamento] ADD [BarbeiroId] BIGINT NULL;
                    END
                ");

                if (!await _context.Barbeiros.AnyAsync())
                {
                    var filiais = await _context.Filiais.ToListAsync();
                    long? filial1Id = filiais.FirstOrDefault()?.Id;
                    long? filial2Id = filiais.Skip(1).FirstOrDefault()?.Id ?? filial1Id;

                    await _context.Barbeiros.AddRangeAsync(
                        new Barbeiro { Nome = "Lucas Silva", FotoUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150", FilialId = filial1Id, Ativo = true },
                        new Barbeiro { Nome = "Marcos Santos", FotoUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150", FilialId = filial1Id, Ativo = true },
                        new Barbeiro { Nome = "Gabriel Oliveira", FotoUrl = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=150", FilialId = filial2Id, Ativo = true }
                    );
                    await _context.SaveChangesAsync();
                }

                _tableChecked = true;
            }
            catch
            {
                // Tolerante para provedores sem suporte a SQL raw (ex: In-Memory em testes)
            }
        }

        public async Task<Response<BarbeiroUsuarioInfoResponse?>> BuscarUsuarioPorEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new Response<BarbeiroUsuarioInfoResponse?>(null, 400, "Informe um e-mail válido para busca.");

                var emailTrimmed = email.Trim();
                var emailNormalized = emailTrimmed.ToUpperInvariant();

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalized || u.Email == emailTrimmed);

                if (user is null)
                {
                    return new Response<BarbeiroUsuarioInfoResponse?>(
                        null,
                        404,
                        $"Nenhum usuário cadastrado encontrado com o e-mail '{emailTrimmed}'. O profissional deve criar uma conta primeiro.");
                }

                var barbeiroExistente = await _context.Barbeiros
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.UsuarioId == user.Id);

                var nomeExibicao = !string.IsNullOrWhiteSpace(user.NomeCompleto)
                    ? user.NomeCompleto
                    : (!string.IsNullOrWhiteSpace(user.UserName) ? user.UserName : "Usuário");

                var info = new BarbeiroUsuarioInfoResponse(
                    user.Id,
                    nomeExibicao,
                    user.Email ?? emailTrimmed,
                    user.PhoneNumber,
                    barbeiroExistente != null,
                    barbeiroExistente?.Nome
                );

                return new Response<BarbeiroUsuarioInfoResponse?>(info, 200);
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroUsuarioInfoResponse?>(null, 500, "Erro ao consultar usuário por e-mail: " + ex.Message);
            }
        }

        public async Task<Response<BarbeiroResponse?>> CreateAsync(CreateBarbeiroRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                if (string.IsNullOrWhiteSpace(request.Email))
                    return new Response<BarbeiroResponse?>(null, 400, "O e-mail do usuário cadastrado é obrigatório.");

                var emailTrimmed = request.Email.Trim();
                var emailNormalized = emailTrimmed.ToUpperInvariant();

                User? user = null;
                if (_userManager != null)
                {
                    user = await _userManager.FindByEmailAsync(emailTrimmed);
                }
                user ??= await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalized || u.Email == emailTrimmed);

                if (user is null)
                {
                    return new Response<BarbeiroResponse?>(null, 404, $"Nenhum usuário encontrado com o e-mail '{emailTrimmed}'. Peça para o profissional criar uma conta primeiro.");
                }

                var jaExiste = await _context.Barbeiros.AnyAsync(b => b.UsuarioId == user.Id);
                if (jaExiste)
                {
                    return new Response<BarbeiroResponse?>(null, 400, "Este usuário já está cadastrado como barbeiro no sistema.");
                }

                const string barbeiroRole = "Barbeiro";
                if (_roleManager != null && !await _roleManager.RoleExistsAsync(barbeiroRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole<long>(barbeiroRole));
                }

                if (_userManager != null && !await _userManager.IsInRoleAsync(user, barbeiroRole))
                {
                    await _userManager.AddToRoleAsync(user, barbeiroRole);
                }

                var nomeFinal = !string.IsNullOrWhiteSpace(request.Nome)
                    ? request.Nome.Trim()
                    : (!string.IsNullOrWhiteSpace(user.NomeCompleto) ? user.NomeCompleto : (user.UserName ?? "Barbeiro"));

                var barbeiro = new Barbeiro
                {
                    Nome = nomeFinal,
                    FotoUrl = request.FotoUrl?.Trim() ?? string.Empty,
                    FilialId = request.FilialId,
                    Ativo = request.Ativo,
                    UsuarioId = user.Id
                };

                await _context.Barbeiros.AddAsync(barbeiro);
                await _context.SaveChangesAsync();

                var filialNome = request.FilialId.HasValue
                    ? (await _context.Filiais.Where(f => f.Id == request.FilialId.Value).Select(f => f.Nome).FirstOrDefaultAsync() ?? "")
                    : "";

                var response = new BarbeiroResponse(barbeiro.Id, barbeiro.Nome, barbeiro.FotoUrl, barbeiro.FilialId, filialNome, barbeiro.Ativo, barbeiro.UsuarioId, user.Email ?? emailTrimmed);
                return new Response<BarbeiroResponse?>(response, 201, "Barbeiro cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroResponse?>(null, 500, "Erro ao cadastrar barbeiro: " + ex.Message);
            }
        }

        public async Task<Response<BarbeiroResponse?>> UpdateAsync(UpdateBarbeiroRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var barbeiro = await _context.Barbeiros.FirstOrDefaultAsync(x => x.Id == request.Id);
                if (barbeiro is null)
                    return new Response<BarbeiroResponse?>(null, 404, "Barbeiro não encontrado.");

                string? userEmail = null;

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var emailTrimmed = request.Email.Trim();
                    var emailNormalized = emailTrimmed.ToUpperInvariant();

                    User? user = null;
                    if (_userManager != null)
                        user = await _userManager.FindByEmailAsync(emailTrimmed);
                    user ??= await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalized || u.Email == emailTrimmed);

                    if (user != null)
                    {
                        const string barbeiroRole = "Barbeiro";
                        if (_roleManager != null && !await _roleManager.RoleExistsAsync(barbeiroRole))
                            await _roleManager.CreateAsync(new IdentityRole<long>(barbeiroRole));

                        if (_userManager != null && !await _userManager.IsInRoleAsync(user, barbeiroRole))
                            await _userManager.AddToRoleAsync(user, barbeiroRole);

                        barbeiro.UsuarioId = user.Id;
                        userEmail = user.Email;
                    }
                }
                else if (request.UsuarioId.HasValue)
                {
                    barbeiro.UsuarioId = request.UsuarioId;
                }

                barbeiro.Nome = request.Nome.Trim();
                barbeiro.FotoUrl = request.FotoUrl?.Trim() ?? string.Empty;
                barbeiro.FilialId = request.FilialId;
                barbeiro.Ativo = request.Ativo;

                await _context.SaveChangesAsync();

                if (userEmail == null && barbeiro.UsuarioId.HasValue)
                {
                    userEmail = await _context.Users
                        .Where(u => u.Id == barbeiro.UsuarioId.Value)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();
                }

                var filialNome = request.FilialId.HasValue
                    ? (await _context.Filiais.Where(f => f.Id == request.FilialId.Value).Select(f => f.Nome).FirstOrDefaultAsync() ?? "")
                    : "";

                var response = new BarbeiroResponse(barbeiro.Id, barbeiro.Nome, barbeiro.FotoUrl, barbeiro.FilialId, filialNome, barbeiro.Ativo, barbeiro.UsuarioId, userEmail ?? "");
                return new Response<BarbeiroResponse?>(response, 200, "Barbeiro atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroResponse?>(null, 500, "Erro ao atualizar barbeiro: " + ex.Message);
            }
        }

        public async Task<Response<BarbeiroResponse?>> DeleteAsync(long id)
        {
            try
            {
                await EnsureTableExistsAsync();

                var barbeiro = await _context.Barbeiros.Include(b => b.Filial).FirstOrDefaultAsync(x => x.Id == id);
                if (barbeiro is null)
                    return new Response<BarbeiroResponse?>(null, 404, "Barbeiro não encontrado.");

                var hasAgendamentos = await _context.Agendamentos.AnyAsync(x => x.BarbeiroId == id);
                if (hasAgendamentos)
                    return new Response<BarbeiroResponse?>(null, 400, "Não é possível excluir um barbeiro que possui agendamentos vinculados.");

                // Se o usuário não estiver vinculado a outro barbeiro, remover o papel Barbeiro
                if (barbeiro.UsuarioId.HasValue && _userManager != null)
                {
                    var outroVinculo = await _context.Barbeiros.AnyAsync(b => b.UsuarioId == barbeiro.UsuarioId && b.Id != id);
                    if (!outroVinculo)
                    {
                        var user = await _userManager.FindByIdAsync(barbeiro.UsuarioId.Value.ToString());
                        if (user != null && await _userManager.IsInRoleAsync(user, "Barbeiro"))
                        {
                            await _userManager.RemoveFromRoleAsync(user, "Barbeiro");
                        }
                    }
                }

                var response = new BarbeiroResponse(barbeiro.Id, barbeiro.Nome, barbeiro.FotoUrl, barbeiro.FilialId, barbeiro.Filial?.Nome ?? "", barbeiro.Ativo, barbeiro.UsuarioId);
                _context.Barbeiros.Remove(barbeiro);
                await _context.SaveChangesAsync();

                return new Response<BarbeiroResponse?>(response, 200, "Barbeiro excluído com sucesso.");
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroResponse?>(null, 500, "Erro ao excluir barbeiro: " + ex.Message);
            }
        }

        public async Task<Response<Barbeiro?>> GetByIdAsync(GetBarbeiroByIdRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var barbeiro = await _context.Barbeiros
                    .AsNoTracking()
                    .Include(x => x.Filial)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (barbeiro != null && barbeiro.UsuarioId.HasValue)
                {
                    barbeiro.Email = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == barbeiro.UsuarioId.Value)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();
                }

                return barbeiro is null
                    ? new Response<Barbeiro?>(null, 404, "Barbeiro não encontrado.")
                    : new Response<Barbeiro?>(barbeiro);
            }
            catch (Exception ex)
            {
                return new Response<Barbeiro?>(null, 500, "Erro ao obter barbeiro: " + ex.Message);
            }
        }

        public async Task<Response<Barbeiro?>> GetByUserIdAsync(long userId)
        {
            try
            {
                await EnsureTableExistsAsync();

                var barbeiro = await _context.Barbeiros
                    .AsNoTracking()
                    .Include(x => x.Filial)
                    .FirstOrDefaultAsync(x => x.UsuarioId == userId);

                if (barbeiro != null && barbeiro.UsuarioId.HasValue)
                {
                    barbeiro.Email = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == barbeiro.UsuarioId.Value)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();
                }

                return barbeiro is null
                    ? new Response<Barbeiro?>(null, 404, "Barbeiro não vinculado ao usuário informado.")
                    : new Response<Barbeiro?>(barbeiro);
            }
            catch (Exception ex)
            {
                return new Response<Barbeiro?>(null, 500, "Erro ao obter barbeiro do usuário: " + ex.Message);
            }
        }

        public async Task<PagedResponse<List<Barbeiro>>> GetAllAsync(GetAllBarbeiroRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var query = _context.Barbeiros
                    .AsNoTracking()
                    .Include(x => x.Filial)
                    .AsQueryable();

                if (request.ApenasAtivos == true)
                {
                    query = query.Where(x => x.Ativo);
                }

                if (request.FilialId.HasValue && request.FilialId.Value > 0)
                {
                    query = query.Where(x => x.FilialId == request.FilialId.Value);
                }

                query = query.OrderBy(x => x.Nome);

                var barbeiros = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var count = await query.CountAsync();

                var userIds = barbeiros.Where(b => b.UsuarioId.HasValue).Select(b => b.UsuarioId!.Value).Distinct().ToList();
                if (userIds.Any())
                {
                    var emails = await _context.Users
                        .AsNoTracking()
                        .Where(u => userIds.Contains(u.Id))
                        .ToDictionaryAsync(u => u.Id, u => u.Email);

                    foreach (var b in barbeiros)
                    {
                        if (b.UsuarioId.HasValue && emails.TryGetValue(b.UsuarioId.Value, out var email))
                        {
                            b.Email = email;
                        }
                    }
                }

                return new PagedResponse<List<Barbeiro>>(barbeiros, count, request.PageNumber, request.PageSize);
            }
            catch (Exception ex)
            {
                return new PagedResponse<List<Barbeiro>>(null, 500, "Falha ao recuperar os barbeiros: " + ex.Message);
            }
        }

        public async Task<Response<BarbeiroDashboardResponse?>> GetDashboardAsync(long? barbeiroId = null)
        {
            try
            {
                await EnsureTableExistsAsync();

                Barbeiro? barbeiro = null;
                if (barbeiroId.HasValue && barbeiroId.Value > 0)
                {
                    barbeiro = await _context.Barbeiros
                        .AsNoTracking()
                        .Include(b => b.Filial)
                        .FirstOrDefaultAsync(b => b.Id == barbeiroId.Value);
                }
                else
                {
                    barbeiro = await _context.Barbeiros
                        .AsNoTracking()
                        .Include(b => b.Filial)
                        .FirstOrDefaultAsync(b => b.Ativo)
                        ?? await _context.Barbeiros
                            .AsNoTracking()
                            .Include(b => b.Filial)
                            .FirstOrDefaultAsync();
                }

                if (barbeiro is null)
                    return new Response<BarbeiroDashboardResponse?>(null, 404, "Nenhum barbeiro encontrado no sistema.");

                var today = DateTime.Today;
                var todayEnd = today.AddDays(1).AddTicks(-1);

                var diffToMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                var weekStart = today.AddDays(-diffToMonday);
                var weekEnd = weekStart.AddDays(7).AddTicks(-1);

                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                // Agendamentos válidos do barbeiro (não cancelados) para métricas e faturamento
                var agendamentosValidos = await _context.Agendamentos
                    .AsNoTracking()
                    .Include(a => a.Corte)
                    .Include(a => a.Filial)
                    .Include(a => a.Barbeiro)
                    .Where(a => a.BarbeiroId == barbeiro.Id && a.Status != EStatusAgendamento.Cancelado)
                    .OrderByDescending(a => a.Data)
                    .ToListAsync();

                var hojeValidos = agendamentosValidos.Where(a => a.Data >= today && a.Data <= todayEnd).ToList();
                var semanaValidos = agendamentosValidos.Where(a => a.Data >= weekStart && a.Data <= weekEnd).ToList();
                var mesValidos = agendamentosValidos.Where(a => a.Data >= monthStart && a.Data <= monthEnd).ToList();

                // Todos os agendamentos do barbeiro (inclusive cancelados/pendentes) para exibição em modo leitura
                var todosAgendamentos = await _context.Agendamentos
                    .AsNoTracking()
                    .Include(a => a.Corte)
                    .Include(a => a.Filial)
                    .Include(a => a.Barbeiro)
                    .Where(a => a.BarbeiroId == barbeiro.Id)
                    .OrderByDescending(a => a.Data)
                    .ToListAsync();

                var userIds = todosAgendamentos.Select(a => a.UserId).Distinct().ToList();
                var usuarios = await _context.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.NomeCompleto);

                foreach (var a in todosAgendamentos)
                {
                    if (usuarios.TryGetValue(a.UserId, out var nomeCliente) && !string.IsNullOrWhiteSpace(nomeCliente))
                    {
                        a.NomeCliente = nomeCliente;
                    }
                    else
                    {
                        a.NomeCliente = "Cliente";
                    }
                }

                var agendamentosHoje = todosAgendamentos
                    .Where(a => a.Data >= today && a.Data <= todayEnd)
                    .Select(a => (AgendamentoResponse)a)
                    .ToList();

                var agendamentosSemana = todosAgendamentos
                    .Where(a => a.Data >= weekStart && a.Data <= weekEnd)
                    .Select(a => (AgendamentoResponse)a)
                    .ToList();

                var agendamentosMes = todosAgendamentos
                    .Where(a => a.Data >= monthStart && a.Data <= monthEnd)
                    .Select(a => (AgendamentoResponse)a)
                    .ToList();

                // Serviço mais trabalhado
                var servicosGroup = agendamentosValidos
                    .GroupBy(a => !string.IsNullOrWhiteSpace(a.DescricaoServicos) ? a.DescricaoServicos : (a.Corte?.Titulo ?? "Sem corte"))
                    .Select(g => new
                    {
                        Titulo = g.Key,
                        Quantidade = g.Count(),
                        TotalReceita = g.Sum(x => x.Valor)
                    })
                    .OrderByDescending(x => x.Quantidade)
                    .ThenByDescending(x => x.TotalReceita)
                    .ToList();

                var topServico = servicosGroup.FirstOrDefault();
                var totalCortes = servicosGroup.Sum(x => x.Quantidade);

                var servicosMaisTrabalhados = servicosGroup.Select(s => new CorteMetricaResponse
                {
                    CorteId = 0,
                    Titulo = s.Titulo,
                    Quantidade = s.Quantidade,
                    TotalReceita = s.TotalReceita,
                    Porcentagem = totalCortes > 0 ? (double)s.Quantidade / totalCortes * 100 : 0
                }).ToList();

                // Serviços executados hoje (reset diário)
                var servicosHojeGroup = hojeValidos
                    .GroupBy(a => !string.IsNullOrWhiteSpace(a.DescricaoServicos) ? a.DescricaoServicos : (a.Corte?.Titulo ?? "Sem corte"))
                    .Select(g => new
                    {
                        Titulo = g.Key,
                        Quantidade = g.Count(),
                        TotalReceita = g.Sum(x => x.Valor)
                    })
                    .OrderByDescending(x => x.Quantidade)
                    .ThenByDescending(x => x.TotalReceita)
                    .ToList();

                var servicosHoje = servicosHojeGroup.Select(s => new CorteMetricaResponse
                {
                    CorteId = 0,
                    Titulo = s.Titulo,
                    Quantidade = s.Quantidade,
                    TotalReceita = s.TotalReceita,
                    Porcentagem = hojeValidos.Count > 0 ? (double)s.Quantidade / hojeValidos.Count * 100 : 0
                }).ToList();

                // 1. Detalhamento de Lucro: Por Turno Hoje
                var lucroPorTurnoHoje = new List<PeriodoLucroItemResponse>
                {
                    new()
                    {
                        Rotulo = "Manhã (08h-12h)",
                        Valor = hojeValidos.Where(a => a.Data.Hour < 12).Sum(a => a.Valor),
                        Quantidade = hojeValidos.Count(a => a.Data.Hour < 12)
                    },
                    new()
                    {
                        Rotulo = "Tarde (12h-17h)",
                        Valor = hojeValidos.Where(a => a.Data.Hour >= 12 && a.Data.Hour < 17).Sum(a => a.Valor),
                        Quantidade = hojeValidos.Count(a => a.Data.Hour >= 12 && a.Data.Hour < 17)
                    },
                    new()
                    {
                        Rotulo = "Noite (17h-19h)",
                        Valor = hojeValidos.Where(a => a.Data.Hour >= 17).Sum(a => a.Valor),
                        Quantidade = hojeValidos.Count(a => a.Data.Hour >= 17)
                    }
                };

                // 2. Gráfico de lucro por dia (últimos 7 dias)
                var lucroPorDia = new List<PeriodoLucroItemResponse>();
                for (int i = 6; i >= 0; i--)
                {
                    var dia = today.AddDays(-i);
                    var diaFim = dia.AddDays(1).AddTicks(-1);
                    var agsDia = agendamentosValidos.Where(a => a.Data >= dia && a.Data <= diaFim).ToList();
                    var diaNomeRaw = dia.ToString("ddd", PtBr);
                    var diaNome = diaNomeRaw.Length > 0 ? char.ToUpper(diaNomeRaw[0]) + diaNomeRaw[1..] : diaNomeRaw;

                    lucroPorDia.Add(new PeriodoLucroItemResponse
                    {
                        Rotulo = $"{diaNome} ({dia:dd/MM})",
                        Valor = agsDia.Sum(a => a.Valor),
                        Quantidade = agsDia.Count
                    });
                }

                // 3. Detalhamento de Lucro: Semanas do Mês Atual
                var lucroPorSemanaMes = new List<PeriodoLucroItemResponse>();
                var totalDiasMes = DateTime.DaysInMonth(today.Year, today.Month);
                var semanasDef = new (int inicio, int fim, string rotulo)[]
                {
                    (1, 7, "Sem 1 (01-07)"),
                    (8, 14, "Sem 2 (08-14)"),
                    (15, 21, "Sem 3 (15-21)"),
                    (22, totalDiasMes, $"Sem 4 (22-{totalDiasMes:D2})")
                };

                foreach (var (inicio, fim, rotulo) in semanasDef)
                {
                    var inicioData = new DateTime(today.Year, today.Month, inicio);
                    var fimData = new DateTime(today.Year, today.Month, fim, 23, 59, 59);
                    var agsSemana = mesValidos.Where(a => a.Data >= inicioData && a.Data <= fimData).ToList();
                    lucroPorSemanaMes.Add(new PeriodoLucroItemResponse
                    {
                        Rotulo = rotulo,
                        Valor = agsSemana.Sum(a => a.Valor),
                        Quantidade = agsSemana.Count
                    });
                }

                // Avaliações do barbeiro vinculadas aos seus agendamentos
                var avaliacoesRaw = await (from av in _context.Avaliacoes.AsNoTracking()
                                           join ag in _context.Agendamentos.AsNoTracking() on av.AgendamentoId equals ag.Id
                                           where ag.BarbeiroId == barbeiro.Id
                                           orderby av.Data descending
                                           select new { av, ag }).ToListAsync();

                var minhasAvaliacoes = new List<AvaliacaoResponse>();
                foreach (var item in avaliacoesRaw)
                {
                    var user = await _context.Users.FindAsync(item.av.UserId);
                    var nomeCliente = !string.IsNullOrWhiteSpace(item.av.NomeCliente)
                        ? item.av.NomeCliente
                        : (user?.NomeCompleto ?? "Cliente");
                    var servicoTitulo = !string.IsNullOrWhiteSpace(item.ag.DescricaoServicos)
                        ? item.ag.DescricaoServicos
                        : (item.ag.Corte?.Titulo ?? "");

                    minhasAvaliacoes.Add(new AvaliacaoResponse(
                        item.av.Id,
                        item.av.UserId,
                        item.av.AgendamentoId,
                        item.av.Estrelas,
                        item.av.Comentario,
                        item.av.Data,
                        nomeCliente,
                        barbeiro.Nome,
                        servicoTitulo,
                        barbeiro.Id
                    ));
                }

                double mediaAvaliacoes = avaliacoesRaw.Count > 0 ? avaliacoesRaw.Average(x => x.av.Estrelas) : 5.0;

                var dashboard = new BarbeiroDashboardResponse
                {
                    BarbeiroId = barbeiro.Id,
                    Nome = barbeiro.Nome,
                    FotoUrl = barbeiro.FotoUrl,
                    FilialNome = barbeiro.Filial?.Nome ?? "",
                    TotalAgendamentosHoje = hojeValidos.Count,
                    TotalAgendamentosSemana = semanaValidos.Count,
                    TotalAgendamentosMes = mesValidos.Count,
                    LucroHoje = hojeValidos.Sum(a => a.Valor),
                    LucroSemana = semanaValidos.Sum(a => a.Valor),
                    LucroMes = mesValidos.Sum(a => a.Valor),
                    ServicoMaisTrabalhadoTitulo = topServico?.Titulo ?? "Nenhum no período",
                    ServicoMaisTrabalhadoQuantidade = topServico?.Quantidade ?? 0,
                    LucroPorTurnoHoje = lucroPorTurnoHoje,
                    LucroPorDia = lucroPorDia,
                    LucroPorSemanaMes = lucroPorSemanaMes,
                    ServicosMaisTrabalhados = servicosMaisTrabalhados,
                    ServicosHoje = servicosHoje,
                    AgendamentosHoje = agendamentosHoje,
                    AgendamentosSemana = agendamentosSemana,
                    AgendamentosMes = agendamentosMes,
                    TodosAgendamentos = todosAgendamentos.Select(a => (AgendamentoResponse)a).ToList(),
                    MediaAvaliacoes = mediaAvaliacoes,
                    TotalAvaliacoes = avaliacoesRaw.Count,
                    MinhasAvaliacoes = minhasAvaliacoes
                };

                return new Response<BarbeiroDashboardResponse?>(dashboard, 200);
            }
            catch (Exception ex)
            {
                return new Response<BarbeiroDashboardResponse?>(null, 500, "Erro ao carregar dashboard do barbeiro: " + ex.Message);
            }
        }
    }
}
