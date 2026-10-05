using BarberShop.Api.Data;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Barbeiro;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Handlers
{
    public class BarbeiroHandler : IBarbeiroHandler
    {
        private readonly BarberShopContext _context;
        private static bool _tableChecked = false;

        public BarbeiroHandler(BarberShopContext context)
        {
            _context = context;
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
                            [Ativo] BIT NOT NULL DEFAULT 1
                        );
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

        public async Task<Response<BarbeiroResponse?>> CreateAsync(CreateBarbeiroRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var barbeiro = new Barbeiro
                {
                    Nome = request.Nome.Trim(),
                    FotoUrl = request.FotoUrl?.Trim() ?? string.Empty,
                    FilialId = request.FilialId,
                    Ativo = request.Ativo
                };

                await _context.Barbeiros.AddAsync(barbeiro);
                await _context.SaveChangesAsync();

                var filialNome = request.FilialId.HasValue
                    ? (await _context.Filiais.Where(f => f.Id == request.FilialId.Value).Select(f => f.Nome).FirstOrDefaultAsync() ?? "")
                    : "";

                var response = new BarbeiroResponse(barbeiro.Id, barbeiro.Nome, barbeiro.FotoUrl, barbeiro.FilialId, filialNome, barbeiro.Ativo);
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

                barbeiro.Nome = request.Nome.Trim();
                barbeiro.FotoUrl = request.FotoUrl?.Trim() ?? string.Empty;
                barbeiro.FilialId = request.FilialId;
                barbeiro.Ativo = request.Ativo;

                await _context.SaveChangesAsync();

                var filialNome = request.FilialId.HasValue
                    ? (await _context.Filiais.Where(f => f.Id == request.FilialId.Value).Select(f => f.Nome).FirstOrDefaultAsync() ?? "")
                    : "";

                var response = new BarbeiroResponse(barbeiro.Id, barbeiro.Nome, barbeiro.FotoUrl, barbeiro.FilialId, filialNome, barbeiro.Ativo);
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

                var response = new BarbeiroResponse(barbeiro.Id, barbeiro.Nome, barbeiro.FotoUrl, barbeiro.FilialId, barbeiro.Filial?.Nome ?? "", barbeiro.Ativo);
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

                return barbeiro is null
                    ? new Response<Barbeiro?>(null, 404, "Barbeiro não encontrado.")
                    : new Response<Barbeiro?>(barbeiro);
            }
            catch (Exception ex)
            {
                return new Response<Barbeiro?>(null, 500, "Erro ao obter barbeiro: " + ex.Message);
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

                return new PagedResponse<List<Barbeiro>>(barbeiros, count, request.PageNumber, request.PageSize);
            }
            catch (Exception ex)
            {
                return new PagedResponse<List<Barbeiro>>(null, 500, "Falha ao recuperar os barbeiros: " + ex.Message);
            }
        }
    }
}
