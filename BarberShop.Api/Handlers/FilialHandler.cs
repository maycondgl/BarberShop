using BarberShop.Api.Data;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Filiais;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Filial;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Handlers
{
    public class FilialHandler : IFilialHandler
    {
        private readonly BarberShopContext _context;
        private static bool _tableChecked = false;

        public FilialHandler(BarberShopContext context)
        {
            _context = context;
        }

        private async Task EnsureTableExistsAsync()
        {
            if (_tableChecked) return;
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Filial')
                    BEGIN
                        CREATE TABLE [Filial] (
                            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Nome] NVARCHAR(100) NOT NULL,
                            [Localizacao] NVARCHAR(200) NOT NULL,
                            [Telefone] NVARCHAR(20) NULL,
                            [Ativo] BIT NOT NULL DEFAULT 1
                        );
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Agendamento' AND COLUMN_NAME = 'FilialId')
                    BEGIN
                        ALTER TABLE [Agendamento] ADD [FilialId] BIGINT NULL;
                    END
                ");

                if (!await _context.Filiais.AnyAsync())
                {
                    await _context.Filiais.AddRangeAsync(
                        new Filial { Nome = "BarberShop - Centro", Localizacao = "Rua Central, 120 - Centro", Telefone = "(11) 99999-1111", Ativo = true },
                        new Filial { Nome = "BarberShop - Shopping", Localizacao = "Av. das Américas, 500 - Piso L2", Telefone = "(11) 98888-2222", Ativo = true }
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

        public async Task<Response<FilialResponse?>> CreateAsync(CreateFilialRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var existe = await _context.Filiais
                    .AnyAsync(x => x.Nome.ToLower() == request.Nome.Trim().ToLower());

                if (existe)
                    return new Response<FilialResponse?>(null, 400, "Já existe uma filial com esse nome.");

                var filial = new Filial
                {
                    Nome = request.Nome.Trim(),
                    Localizacao = request.Localizacao.Trim(),
                    Telefone = request.Telefone?.Trim() ?? string.Empty,
                    Ativo = request.Ativo
                };

                await _context.Filiais.AddAsync(filial);
                await _context.SaveChangesAsync();

                var response = new FilialResponse(filial.Id, filial.Nome, filial.Localizacao, filial.Telefone, filial.Ativo);
                return new Response<FilialResponse?>(response, 201, "Filial criada com sucesso.");
            }
            catch (Exception ex)
            {
                return new Response<FilialResponse?>(null, 500, "Erro ao criar filial: " + ex.Message);
            }
        }

        public async Task<Response<FilialResponse?>> UpdateAsync(UpdateFilialRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var filial = await _context.Filiais.FirstOrDefaultAsync(x => x.Id == request.Id);
                if (filial is null)
                    return new Response<FilialResponse?>(null, 404, "Filial não encontrada.");

                var nomeDuplicado = await _context.Filiais
                    .AnyAsync(x => x.Id != request.Id && x.Nome.ToLower() == request.Nome.Trim().ToLower());

                if (nomeDuplicado)
                    return new Response<FilialResponse?>(null, 400, "Já existe outra filial cadastrada com esse nome.");

                filial.Nome = request.Nome.Trim();
                filial.Localizacao = request.Localizacao.Trim();
                filial.Telefone = request.Telefone?.Trim() ?? string.Empty;
                filial.Ativo = request.Ativo;

                await _context.SaveChangesAsync();

                var response = new FilialResponse(filial.Id, filial.Nome, filial.Localizacao, filial.Telefone, filial.Ativo);
                return new Response<FilialResponse?>(response, 200, "Filial atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return new Response<FilialResponse?>(null, 500, "Erro ao atualizar filial: " + ex.Message);
            }
        }

        public async Task<Response<FilialResponse?>> DeleteAsync(long id)
        {
            try
            {
                await EnsureTableExistsAsync();

                var filial = await _context.Filiais.FirstOrDefaultAsync(x => x.Id == id);
                if (filial is null)
                    return new Response<FilialResponse?>(null, 404, "Filial não encontrada.");

                var hasAgendamentos = await _context.Agendamentos.AnyAsync(x => x.FilialId == id);
                if (hasAgendamentos)
                    return new Response<FilialResponse?>(null, 400, "Não é possível excluir uma filial que possui agendamentos vinculados.");

                var response = new FilialResponse(filial.Id, filial.Nome, filial.Localizacao, filial.Telefone, filial.Ativo);
                _context.Filiais.Remove(filial);
                await _context.SaveChangesAsync();

                return new Response<FilialResponse?>(response, 200, "Filial excluída com sucesso.");
            }
            catch (Exception ex)
            {
                return new Response<FilialResponse?>(null, 500, "Erro ao excluir filial: " + ex.Message);
            }
        }

        public async Task<Response<Filial?>> GetByIdAsync(GetFilialByIdRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var filial = await _context.Filiais
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                return filial is null
                    ? new Response<Filial?>(null, 404, "Filial não encontrada.")
                    : new Response<Filial?>(filial);
            }
            catch (Exception ex)
            {
                return new Response<Filial?>(null, 500, "Erro ao obter filial: " + ex.Message);
            }
        }

        public async Task<PagedResponse<List<Filial>>> GetAllAsync(GetAllFilialRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var query = _context.Filiais
                    .AsNoTracking();

                if (request.ApenasAtivos == true)
                {
                    query = query.Where(x => x.Ativo);
                }

                query = query.OrderBy(x => x.Nome);

                var filiais = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var count = await query.CountAsync();

                return new PagedResponse<List<Filial>>(filiais, count, request.PageNumber, request.PageSize);
            }
            catch (Exception ex)
            {
                return new PagedResponse<List<Filial>>(null, 500, "Falha ao recuperar as filiais: " + ex.Message);
            }
        }
    }
}
