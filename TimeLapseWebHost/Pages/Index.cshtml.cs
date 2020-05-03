using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TimeLapseWebHost.Pages
{
    //todo: moeten we echt even fixen haha (razorpages eis)
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IFileStore _fileStore;
        private readonly IVideoEngine _videoEngine;
        public string UserId { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IFileStore fileStore, IVideoEngine videoEngine)
        {
            _logger = logger;
            _fileStore = fileStore;
            _videoEngine = videoEngine;
        }

        public void OnGet()
        {
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [BindProperty, Display(Name = "File")]
        public IFormFile UploadedFile { get; set; }
        public async Task<IActionResult> OnPost()
        {
            if (UploadedFile == null)
            {
                return Page();
            }
            if (User == null)
            {
                throw new UnauthorizedAccessException();
            }
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
            {
                throw new UnauthorizedAccessException();
            }
            //bewust niet awaiten? laat maar gaan
            await _fileStore.Create(UploadedFile, id);
            await _videoEngine.Create(id);

            return RedirectToPage("/Index");
        }
    }
}
