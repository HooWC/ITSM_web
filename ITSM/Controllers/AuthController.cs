﻿using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
