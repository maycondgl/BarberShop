using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.HorariosFuncionamento;
using BarberShop.Core.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BarberShop.Web.Handlers
{
    public class HorarioFuncionamentoHandler(IHttpClientFactory httpClientFactory) : IHorarioFuncionamentoHandler
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

        public async Task<Response<List<HorarioFuncionamentoDto>>> GetByFilialAsync(long filialId)
        {
            try
            {
                var response = await _client.GetFromJsonAsync<Response<List<HorarioFuncionamentoDto>>>($"v1/filiais/{filialId}/horarios");
                return response ?? new Response<List<HorarioFuncionamentoDto>>([], 400, "Falha ao obter os horários da filial.");
            }
            catch (Exception ex)
            {
                return new Response<List<HorarioFuncionamentoDto>>([], 500, "Erro de comunicação: " + ex.Message);
            }
        }

        public async Task<Response<bool>> SaveHorariosAsync(SalvarHorariosFilialRequest request)
        {
            try
            {
                var result = await _client.PutAsJsonAsync($"v1/filiais/{request.FilialId}/horarios", request);
                return await result.Content.ReadFromJsonAsync<Response<bool>>()
                    ?? new Response<bool>(false, 400, "Falha ao salvar horários da filial.");
            }
            catch (Exception ex)
            {
                return new Response<bool>(false, 500, "Erro de comunicação: " + ex.Message);
            }
        }
    }
}
