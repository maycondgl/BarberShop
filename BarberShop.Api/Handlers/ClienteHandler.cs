using BarberShop.Api.Data;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Clientes;
using BarberShop.Core.Responses;
using BarberShop.Core.Responses.Cliente;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.Api.Handlers
{
    public class ClienteHandler : IClienteHandler
    {
        private readonly BarberShopContext _context;

        public ClienteHandler(BarberShopContext context)
        {
            _context = context;
        }
        public async Task<Response<ClienteResponse?>> CreateAsync(CreateClienteRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Nome))
                    return new Response<ClienteResponse?>(
                        null,
                        400,
                        "O nome é obrigatório."
                    );

                if (string.IsNullOrWhiteSpace(request.Telefone))
                    return new Response<ClienteResponse?>(
                        null,
                        400,
                        "O telefone é obrigatório."
                    );

                if (request.Telefone.Length < 10)
                    return new Response<ClienteResponse?>(
                        null,
                        400,
                        "Telefone inválido."
                    );

                var cliente = new Cliente
                {
                    Nome = request.Nome,
                    Telefone = request.Telefone,
                    UserId = request.UserId
                };

                await _context.Clientes.AddAsync(cliente);
                await _context.SaveChangesAsync();

                var response = new ClienteResponse(
                      cliente.Id,
                      cliente.Nome,
                      cliente.Telefone
                  );
                return new Response<ClienteResponse?>(response, 201, "Cliente registrado com sucesso!");
            }
            catch
            {
                return new Response<ClienteResponse?>(null, 400, "Falha ao criar cliente");
            }
        }

        public async Task<Response<ClienteResponse?>> UpdateAsync(UpdateClienteRequest request)
        {
            try
            {
                var cliente = await _context.Clientes
            .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (cliente is null)
                    return new Response<ClienteResponse?>(
                        null,
                        404,
                        "Cliente não encontrado"
                    );

                cliente.Nome = request.Nome;
                cliente.Telefone = request.Telefone;

                await _context.SaveChangesAsync();

                var response = new ClienteResponse(
                    cliente.Id,
                    cliente.Nome,
                    cliente.Telefone);

                return new Response<ClienteResponse?>(response, 200, "Cliente atualizado com sucesso");
            }
            catch
            {
                return new Response<ClienteResponse?>(null, 400, "Falha ao atualizar cliente");

            }
        }
        public async Task<Response<ClienteResponse?>> DeleteAsync(long id)
        {
            try
            {
                var cliente = await _context
                  .Clientes
                  .FirstOrDefaultAsync(x => x.Id == id);

                if (cliente is null)
                    return new Response<ClienteResponse?>(null, 404, "Cliente não encontrado");

                var response = new ClienteResponse(
                        cliente.Id,
                        cliente.Nome,
                        cliente.Telefone
                    );

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                return new Response<ClienteResponse?>(response, message: "Cliente excluído com sucesso");
            }
            catch
            {
                return new Response<ClienteResponse?>(null, 500, "Não foi possível excluir o cliente");
            }
        }

        public async Task<Response<Cliente?>> GetByIdAsync(GetClienteByIdRequest request)
        {
            try
            {
                var cliente = await _context
                   .Clientes
                   .AsNoTracking()
                   .FirstOrDefaultAsync(x => x.Id == request.Id);

                return cliente is null
                    ? new Response<Cliente?>(null, 404, "Avaliação não encontrada")
                    : new Response<Cliente?>(cliente);
            }
            catch
            {
                return new Response<Cliente?>(null, 500, "Não foi possível recuperar o cliente");
            }
        }

        public async Task<PagedResponse<List<Cliente>>> GetAllAsync(GetAllClienteRequest request)
        {
            try
            {
                var query = _context
                   .Clientes
                   .AsNoTracking()
                   .OrderBy(x => x.Id);

                var clientes = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var count = await query.CountAsync();

                return new PagedResponse<List<Cliente>>(clientes,
                    count,
                    request.PageNumber,
                    request.PageSize);
            }
            catch
            {
                return new PagedResponse<List<Cliente>>(null, 500, "Falha ao recuperar os clientes");

            }
        }       
    }
}
