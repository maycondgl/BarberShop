using BarberShop.Api.Data;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.DiasFechados;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.DiasFechados;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Handlers
{
    public class DiaFechadoHandler : IDiaFechadoHandler
    {
        private readonly BarberShopContext _context;
        private static bool _tableChecked = false;

        public DiaFechadoHandler(BarberShopContext context)
        {
            _context = context;
        }

        private async Task EnsureTableExistsAsync()
        {
            if (_tableChecked) return;
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DiaFechado')
                    BEGIN
                        CREATE TABLE [DiaFechado] (
                            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Data] DATETIME2 NOT NULL,
                            [Motivo] NVARCHAR(150) NULL
                        );
                    END");
                _tableChecked = true;
            }
            catch
            {
                // Tolerante caso o provedor de banco não suporte ExecuteSqlRaw (ex: In-Memory nos testes)
            }
        }

        public async Task<Response<DiaFechadoResponse?>> CreateAsync(CreateDiaFechadoRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var dataDate = request.Data.Date;
                var existe = await _context.DiasFechados
                    .AnyAsync(x => x.Data.Date == dataDate);

                if (existe)
                    return new Response<DiaFechadoResponse?>(null, 400, "Esta data já está cadastrada como dia fechado");

                var diaFechado = new DiaFechado
                {
                    Data = dataDate,
                    Motivo = request.Motivo?.Trim() ?? string.Empty
                };

                await _context.DiasFechados.AddAsync(diaFechado);
                await _context.SaveChangesAsync();

                var response = new DiaFechadoResponse(
                    diaFechado.Id,
                    diaFechado.Data,
                    diaFechado.Motivo
                );

                return new Response<DiaFechadoResponse?>(response, 201, "Dia fechado cadastrado com sucesso");
            }
            catch
            {
                return new Response<DiaFechadoResponse?>(null, 500, "Erro ao cadastrar dia fechado");
            }
        }

        public async Task<Response<DiaFechadoResponse?>> DeleteAsync(long id)
        {
            try
            {
                await EnsureTableExistsAsync();

                var diaFechado = await _context.DiasFechados
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (diaFechado is null)
                    return new Response<DiaFechadoResponse?>(null, 404, "Registro de dia fechado não encontrado");

                _context.DiasFechados.Remove(diaFechado);
                await _context.SaveChangesAsync();

                var response = new DiaFechadoResponse(
                    diaFechado.Id,
                    diaFechado.Data,
                    diaFechado.Motivo
                );

                return new Response<DiaFechadoResponse?>(response, 200, "Dia fechado removido com sucesso");
            }
            catch
            {
                return new Response<DiaFechadoResponse?>(null, 500, "Erro ao remover dia fechado");
            }
        }

        public async Task<PagedResponse<List<DiaFechado>>> GetAllAsync(GetAllDiasFechadosRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var query = _context.DiasFechados
                    .AsNoTracking()
                    .OrderBy(x => x.Data);

                var count = await query.CountAsync();

                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return new PagedResponse<List<DiaFechado>>(items, count, request.PageNumber, request.PageSize);
            }
            catch
            {
                // Em caso de tabela ainda não inicializada ou erro temporário, retorna lista vazia segura
                return new PagedResponse<List<DiaFechado>>(new List<DiaFechado>(), 0, request.PageNumber, request.PageSize);
            }
        }
    }
}
