using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class UrlController : Controller
    {
        private readonly AppDbContext _context;

        public UrlController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Urls.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(string originalUrl)
        {
            if (!Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
            {
                return RedirectToAction("Index");
            }

            var shortCode = ShortUrlGenerator.Generate();

            var url = new Url
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.Now,
                ClickCount = 0
            };

            _context.Add(url);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var url = await _context.Urls
                .FirstOrDefaultAsync(x => x.ShortCode == code);

            if (url == null)
                return NotFound();

            url.ClickCount++;
            await _context.SaveChangesAsync();

            return Redirect(url.OriginalUrl);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var url = await _context.Urls.FindAsync(id);

            if (url != null)
            {
                _context.Remove(url);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var url = await _context.Urls.FindAsync(id);
            if (url == null)
            {
                return NotFound();
            }
            return View(url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OriginalUrl")] Url url)
        {
            if (id != url.Id)
            {
                return NotFound();
            }

            if (!Uri.IsWellFormedUriString(url.OriginalUrl, UriKind.Absolute))
            {
                ModelState.AddModelError("OriginalUrl", "¬ведите корректный абсолютный URL (например, https://example.com).");
                return View(url);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUrl = await _context.Urls.FindAsync(id);
                    if (existingUrl == null)
                    {
                        return NotFound();
                    }

                    existingUrl.OriginalUrl = url.OriginalUrl;

                    _context.Update(existingUrl);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UrlExists(url.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(url);
        }

        private bool UrlExists(int id)
        {
            return _context.Urls.Any(e => e.Id == id);
        }
    }
}