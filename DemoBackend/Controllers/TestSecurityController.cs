using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DemoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestSecurityController(IAntiforgery antiforgery) : ControllerBase
    {
        [HttpGet]
        [Route("antiforgery-token")]
        public IActionResult GetAntiForgeryToken()
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);

            Response.Headers.Add("X-CSRF-Token", tokens.RequestToken!);

            return Ok();
        }

        [HttpPost]
        [Route("pay-tax")]
        public async Task<IActionResult> PayTax([FromBody] PayTaxRequest request)
        {
            try
            {
               await antiforgery.ValidateRequestAsync(HttpContext);

                // Your action logic here
                return Ok("Tax payed successfully");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("account-balance")]
        [EnableRateLimiting("fixed")]
        public IActionResult GetAccountBalance() => Ok("Total balance is 100");
    }

    public record PayTaxRequest
    {
        public string Name { get; init; }
        public double Amount { get; init; }
    }
}
