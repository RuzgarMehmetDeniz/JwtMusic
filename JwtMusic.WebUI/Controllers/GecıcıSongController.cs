using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtMusic.WebUI.Controllers
{
    public class GecıcıSongController : Controller
    {
        private readonly JwtContext _context;

        public GecıcıSongController(JwtContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME & AKILLI SAYFALAMA
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5;
            var totalSongs = await _context.Songs.CountAsync();

            var songs = await _context.Songs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalSongs / pageSize);

            return View(songs);
        }

        // 2. EKLEME (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. EKLEME (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Song song)
        {
            if (ModelState.IsValid)
            {
                _context.Songs.Add(song);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(song);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1)
        {
            var song = await _context.Songs.FirstOrDefaultAsync(x => x.SongId == id);
            if (song == null) return NotFound();

            ViewBag.ReturnPage = page; // Geldikleri sayfayı hafızada tutuyoruz
            return View(song);
        }

        // GÜNCELLEME (POST) - Güncelleme bitince gelinen sayfaya geri gönderiyoruz
        [HttpPost]
        public async Task<IActionResult> Edit(Song song, int returnPage = 1)
        {
            if (ModelState.IsValid)
            {
                _context.Songs.Update(song);
                await _context.SaveChangesAsync();

                // Güncelleme bitince şarkının düzenlendiği sayfaya geri yönlendirir
                return RedirectToAction(nameof(Index), new { page = returnPage });
            }
            ViewBag.ReturnPage = returnPage;
            return View(song);
        }

        // 6. SİLME
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var song = await _context.Songs.FirstOrDefaultAsync(x => x.SongId == id);
            if (song != null)
            {
                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}