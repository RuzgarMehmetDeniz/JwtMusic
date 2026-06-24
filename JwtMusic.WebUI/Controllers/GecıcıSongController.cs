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

        // 1. LİSTELEME & AKILLI SAYFALAMA (Aynen Korundu)
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

        // 4. GÜNCELLEME (GET)
        [HttpGet]
        public async Task<IActionResult> UpdateSong(int id)
        {
            var value = await _context.Songs.FirstOrDefaultAsync(x => x.SongId == id);
            if (value == null)
            {
                return NotFound();
            }
            return View(value);
        }

        // 5. GÜNCELLEME (POST) - Sayfalama parametreleri kaldırıldı, direkt Index'e döner
        [HttpPost]
        public async Task<IActionResult> UpdateSong(Song song)
        {
            var value = await _context.Songs.FirstOrDefaultAsync(x => x.SongId == song.SongId);

            value.Title = song.Title;
            value.CoverImageUrl = song.CoverImageUrl;
            value.AudioUrl = song.AudioUrl;
            value.Duration = song.Duration;
            value.PlayCount = song.PlayCount;
            value.IsPremium = song.IsPremium;
            value.ReleaseDate = song.ReleaseDate;
            value.ArtistId = song.ArtistId;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 6. SİLME (POST)
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