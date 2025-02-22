using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsResevation.UI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SportsResevation.UI.Controllers
{
    public class PaymentController : Controller
    {
        private readonly HttpClient _httpClient;

        public PaymentController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PaymentIyzico()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (User.Identity.IsAuthenticated)
            {
                var accessToken = User.Claims.FirstOrDefault(x => x.Type == "access_token")?.Value;

                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                ViewData["AccessToken"] = accessToken;
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PaymentIyzico(PaymentIyzico paymentIyzico)
        {
            if (!ModelState.IsValid)
            {
                return View(paymentIyzico);
            }
            string jsonString = JsonSerializer.Serialize(paymentIyzico);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (User.Identity.IsAuthenticated)
            {
                var accessToken = User.Claims.FirstOrDefault(x => x.Type == "access_token")?.Value;

                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                ViewData["AccessToken"] = accessToken;
            }
            var result = await _httpClient.PostAsync("api/Payment/", content);

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync();
                return Redirect(String.Format("~/Account/Login?ReturnUrl=/{0}/{1}", ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName));
            }
            var responseModel = JsonSerializer.Deserialize<ResponseMainModel<PaymentIyzico>>(await result.Content.ReadAsStringAsync(), options);

            if (result.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                foreach (var error in responseModel.Errors)
                {
                    ModelState.AddModelError("other", error);
                    return View(paymentIyzico);
                }
            }
            return View(paymentIyzico);
        }
    }
}
