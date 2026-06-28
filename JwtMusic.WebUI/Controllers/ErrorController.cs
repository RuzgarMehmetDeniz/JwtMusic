using Microsoft.AspNetCore.Mvc;

namespace JwtMusic.WebUI.Controllers
{
    public class ErrorController : Controller
    {
        // 404 — Sayfa bulunamadı
        [Route("Error/NotFound")]
        public IActionResult NotFound()
        {
            return View();
        }

        // 500 — Sunucu hatası
        [Route("Error/ServerError")]
        public IActionResult ServerError()
        {
            return View();
        }

        // 401 — Oturum süresi doldu / yetkisiz
        [Route("Error/Unauthorized")]
        public IActionResult Unauthorized()
        {
            return View();
        }
    }
}
