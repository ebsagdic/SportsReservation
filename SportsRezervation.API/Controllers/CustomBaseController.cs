﻿using Microsoft.AspNetCore.Mvc;
using SportsReservation.Core.Models.DTO_S;

namespace SportsReservation.API.Controllers
{
    public class CustomBaseController : ControllerBase
    {
        public IActionResult ActionResultInstance<T>(Response<T> response) where T : class
        {
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };
        }
    }
}
