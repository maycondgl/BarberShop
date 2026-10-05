using BarberShop.Api.Data;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.HorariosFuncionamento;
using BarberShop.Core.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Api.Handlers
{
    public class HorarioFuncionamentoHandler : IHorarioFuncionamentoHandler
    {
        private readonly BarberShopContext _context;
        private static bool _tableChecked = false;

        public HorarioFuncionamentoHandler(BarberShopContext context)
        {
            _context = context;
        }

        private async Task EnsureTableExistsAsync()
        {
            if (_tableChecked) return;

            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HorarioFuncionamento')
                    BEGIN
                        CREATE TABLE [HorarioFuncionamento] (
                            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [FilialId] BIGINT NOT NULL,
                            [DiaSemana] INT NOT NULL,
                            [Aberto] BIT NOT NULL DEFAULT 1,
                            [HorarioAbertura] TIME NOT NULL,
                            [HorarioFechamento] TIME NOT NULL,
                            [TemAlmoco] BIT NOT NULL DEFAULT 1,
                            [AlmocoInicio] TIME NULL,
                            [AlmocoFim] TIME NULL,
                            CONSTRAINT [FK_HorarioFuncionamento_Filial] FOREIGN KEY ([FilialId]) REFERENCES [Filial]([Id]) ON DELETE CASCADE
                        );
                    END
                ");
                _tableChecked = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HorarioFuncionamentoHandler EnsureTableExistsAsync]: {ex.Message}");
            }
        }

        public async Task<Response<List<HorarioFuncionamentoDto>>> GetByFilialAsync(long filialId)
        {
            try
            {
                await EnsureTableExistsAsync();

                var filialExiste = await _context.Filiais.AnyAsync(f => f.Id == filialId);
                if (!filialExiste)
                {
                    return new Response<List<HorarioFuncionamentoDto>>(null, 404, "Filial não encontrada.");
                }

                var existentes = await _context.HorariosFuncionamento
                    .Where(h => h.FilialId == filialId)
                    .ToListAsync();

                if (!existentes.Any())
                {
                    // Inicializar os 7 dias da semana com padrões da barbearia
                    var dias = new[]
                    {
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Thursday,
                        DayOfWeek.Friday,
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    };

                    foreach (var dia in dias)
                    {
                        var horario = new HorarioFuncionamento
                        {
                            FilialId = filialId,
                            DiaSemana = dia,
                            Aberto = dia != DayOfWeek.Sunday,
                            HorarioAbertura = dia == DayOfWeek.Saturday ? new TimeSpan(7, 0, 0) : new TimeSpan(8, 0, 0),
                            HorarioFechamento = dia == DayOfWeek.Saturday ? new TimeSpan(12, 0, 0) : new TimeSpan(19, 0, 0),
                            TemAlmoco = dia != DayOfWeek.Saturday && dia != DayOfWeek.Sunday,
                            AlmocoInicio = (dia != DayOfWeek.Saturday && dia != DayOfWeek.Sunday) ? new TimeSpan(12, 0, 0) : null,
                            AlmocoFim = (dia != DayOfWeek.Saturday && dia != DayOfWeek.Sunday) ? new TimeSpan(13, 0, 0) : null
                        };
                        _context.HorariosFuncionamento.Add(horario);
                        existentes.Add(horario);
                    }

                    await _context.SaveChangesAsync();
                }

                // Ordenar por Segunda (1) até Domingo (0)
                var dtos = existentes
                    .OrderBy(h => h.DiaSemana == DayOfWeek.Sunday ? 7 : (int)h.DiaSemana)
                    .Select(h => new HorarioFuncionamentoDto
                    {
                        Id = h.Id,
                        FilialId = h.FilialId,
                        DiaSemana = h.DiaSemana,
                        NomeDiaSemana = HorarioFuncionamentoDto.ObterNomeDia(h.DiaSemana),
                        Aberto = h.Aberto,
                        HorarioAbertura = h.HorarioAbertura,
                        HorarioFechamento = h.HorarioFechamento,
                        TemAlmoco = h.TemAlmoco,
                        AlmocoInicio = h.AlmocoInicio,
                        AlmocoFim = h.AlmocoFim
                    })
                    .ToList();

                return new Response<List<HorarioFuncionamentoDto>>(dtos, 200);
            }
            catch (Exception ex)
            {
                return new Response<List<HorarioFuncionamentoDto>>(null, 500, "Erro ao obter horários da filial: " + ex.Message);
            }
        }

        public async Task<Response<bool>> SaveHorariosAsync(SalvarHorariosFilialRequest request)
        {
            try
            {
                await EnsureTableExistsAsync();

                var filialExiste = await _context.Filiais.AnyAsync(f => f.Id == request.FilialId);
                if (!filialExiste)
                {
                    return new Response<bool>(false, 404, "Filial não encontrada.");
                }

                var existentes = await _context.HorariosFuncionamento
                    .Where(h => h.FilialId == request.FilialId)
                    .ToListAsync();

                foreach (var item in request.Horarios)
                {
                    // Validação de horários
                    if (item.Aberto)
                    {
                        if (item.HorarioFechamento <= item.HorarioAbertura)
                        {
                            return new Response<bool>(false, 400, $"No dia {HorarioFuncionamentoDto.ObterNomeDia(item.DiaSemana)}, o horário de fechamento deve ser maior que o de abertura.");
                        }

                        if (item.TemAlmoco)
                        {
                            if (!item.AlmocoInicio.HasValue || !item.AlmocoFim.HasValue)
                            {
                                return new Response<bool>(false, 400, $"No dia {HorarioFuncionamentoDto.ObterNomeDia(item.DiaSemana)}, informe o início e fim do almoço.");
                            }

                            if (item.AlmocoFim <= item.AlmocoInicio)
                            {
                                return new Response<bool>(false, 400, $"No dia {HorarioFuncionamentoDto.ObterNomeDia(item.DiaSemana)}, o término do almoço deve ser maior que o início.");
                            }

                            if (item.AlmocoInicio < item.HorarioAbertura || item.AlmocoFim > item.HorarioFechamento)
                            {
                                return new Response<bool>(false, 400, $"No dia {HorarioFuncionamentoDto.ObterNomeDia(item.DiaSemana)}, o intervalo de almoço deve estar dentro do horário de funcionamento.");
                            }
                        }
                    }

                    var existente = existentes.FirstOrDefault(h => h.DiaSemana == item.DiaSemana);
                    if (existente == null)
                    {
                        existente = new HorarioFuncionamento
                        {
                            FilialId = request.FilialId,
                            DiaSemana = item.DiaSemana
                        };
                        _context.HorariosFuncionamento.Add(existente);
                    }

                    existente.Aberto = item.Aberto;
                    existente.HorarioAbertura = item.HorarioAbertura;
                    existente.HorarioFechamento = item.HorarioFechamento;
                    existente.TemAlmoco = item.TemAlmoco;
                    existente.AlmocoInicio = item.TemAlmoco ? item.AlmocoInicio : null;
                    existente.AlmocoFim = item.TemAlmoco ? item.AlmocoFim : null;
                }

                await _context.SaveChangesAsync();
                return new Response<bool>(true, 200, "Horários de funcionamento atualizados com sucesso.");
            }
            catch (Exception ex)
            {
                return new Response<bool>(false, 500, "Erro ao salvar horários da filial: " + ex.Message);
            }
        }
    }
}
