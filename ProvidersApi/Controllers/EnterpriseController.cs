using Azure.Core;
using DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ProvidersApi.Controllers
{
    public class EnterpriseController : Controller
    {
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<EnterpriseController> _logger;

        public EnterpriseController(IOptions<ApiSettings> options, ILogger<EnterpriseController> logger)
        {
            _apiSettings = options.Value;
            _logger = logger;
        }

        [HttpPost("api/enterprises")]
        public IEnumerable<Enterprise> GetAll()
        {
            if (!IsApiKeyValid())
                return new List<Enterprise>();

            return new List<Enterprise>();
        }

        private bool IsApiKeyValid()
        {
            var request_key = Request.Headers
                .Where(h => h.Key == "X-Api-Key")
                .FirstOrDefault().Value;

            var apiKey = _apiSettings?.ApiKey;

            return apiKey == request_key;
        }

    }
}
