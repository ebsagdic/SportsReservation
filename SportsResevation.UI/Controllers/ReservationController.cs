using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsResevation.UI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SportsResevation.UI.Controllers
{
    public class ReservationController : Controller
    {
        private readonly HttpClient _httpClient;
        //Interface'i doğrudan alan olarak saklamanın yerine, somut HttpClient sınıfı saklanıyor.
        //Birden fazla yerde API çağrısı yapmak istediğinizde, her seferinde CreateClient çağırmanıza gerek kalmaz

        public ReservationController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllReservations() 
        {
            ReservationInfoListModel reservationList = new ReservationInfoListModel();
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

            var result = await _httpClient.GetAsync("api/Reservation/");

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync();
                return Redirect(String.Format("~/Account/Login?ReturnUrl=/{0}/{1}", ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName));
            }
            
            var responseModel = JsonSerializer.Deserialize<ResponseMainModel<List<ReservationInfo>>>(await result.Content.ReadAsStringAsync(), options);
            reservationList.Reservations = responseModel.Data.ToList();
            return View(reservationList);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyReservation()
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

            var result = await _httpClient.GetAsync("api/Reservation/Reservasyonum");

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync();
                return Redirect(String.Format("~/Account/Login?ReturnUrl=/{0}/{1}", ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName));
            }

            var responseModel = JsonSerializer.Deserialize<ResponseMainModel<ReservationInfoWithPaidInfo>>(await result.Content.ReadAsStringAsync(), options);

            if (responseModel == null || responseModel.Data == null)
            {
                return View(new ReservationInfoWithPaidInfo()); // Boş model gönder
            }

            return View(responseModel.Data);
        }

    }
}
