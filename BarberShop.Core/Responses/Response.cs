using System.Text.Json.Serialization;

namespace BarberShop.Core.Responses
{
    public class Response<TData>
    {
        [JsonConstructor]
        public Response(
            TData? data, 
            int code = Configuration.DefaultCode, 
            string? message = null)
        {
            Data = data;
            Message = message;
            Code = code;
        }

        public Response()
        {
            Code = Configuration.DefaultCode;
        }

        public TData? Data { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; } = Configuration.DefaultCode;

        [JsonIgnore]
        public bool IsSuccess => Code is >= 200 and <= 299;
    }
}
