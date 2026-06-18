using BarberShop.Core.Responses.Agendamento;
using BarberShop.Web.Security;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;

namespace BarberShop.Web.Services;

public sealed class AdminNotificationClient(
    ISnackbar snackbar,
    IJSRuntime jsRuntime) : IAsyncDisposable
{
    private HubConnection? _hubConnection;

    public async Task StartAsync()
    {
        if (_hubConnection is not null)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(
                $"{Configuration.BackendUrl.TrimEnd('/')}/hubs/agendamentos",
                options =>
                {
                    options.HttpMessageHandlerFactory = innerHandler => new CookieHandler
                    {
                        InnerHandler = innerHandler
                    };
                })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<AgendamentoResponse>("NovoAgendamento", async agendamento =>
        {
            snackbar.Add(
                $"Novo agendamento: {agendamento.NomeCliente} - {agendamento.Data:dd/MM HH:mm}",
                Severity.Info);

            await jsRuntime.InvokeVoidAsync(
                "barberShopNotifications.showLocalNotification",
                "Novo agendamento",
                $"{agendamento.NomeCliente} marcou {agendamento.CorteTitulo} para {agendamento.Data:dd/MM/yyyy HH:mm}.",
                "/admin/agendamentos");
        });

        await _hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }
}
