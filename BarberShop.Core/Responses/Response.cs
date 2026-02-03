using System.Text.Json.Serialization;

namespace BarberShop.Core.Responses
{
    public class Response<TData>
    {
        
        private readonly int _code;

        [JsonConstructor]
        public Response() => _code = Configuration.DefaultCode;

        public Response(
            TData? data, 
            int code = Configuration.DefaultCode, 
            string? message = null)
        {
            Data = data;
            Message = message;
            _code = code;
        }
        public TData? Data { get; set; }
        public string? Message { get; set; }

        [JsonIgnore]
        public bool IsSuccess => _code is >= 200 and <= 299;
    }
}
