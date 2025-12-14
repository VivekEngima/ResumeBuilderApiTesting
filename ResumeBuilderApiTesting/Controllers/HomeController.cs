using Microsoft.AspNetCore.Mvc;
using ResumeBuilderApiTesting.Services;

namespace ResumeBuilderApiTesting.Controllers
{
    public class HomeController : Controller
    {
        private readonly ResumeExtractionService _resumeService;
        private readonly CoverLetterExtractionService _coverLetterService;

        public HomeController(ResumeExtractionService resumeService, CoverLetterExtractionService coverLetterService)
        {
            _resumeService = resumeService;
            _coverLetterService = coverLetterService;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0 || !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Please upload a valid PDF file.";
                return View("Index");
            }

            try
            {
                var json = await _resumeService.ParseResumeToJsonAsync(file);
                ViewBag.Json = json;
                ViewBag.Type = "resume";
                return View("Result");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCoverLetter(IFormFile file)
        {
            if (file == null || file.Length == 0 || !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Please upload a valid PDF file.";
                return View("Index");
            }

            try
            {
                var json = await _coverLetterService.GenerateCoverLetterFromResumeAsync(file);
                ViewBag.Json = json;
                ViewBag.Type = "coverletter";
                return View("Result");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestPerplexityAI()
        {
            try
            {
                var result = await _resumeService.TestPerplexityAsync();
                ViewBag.TestResult = result;
            }
            catch (Exception ex)
            {
                ViewBag.TestResult = $"Error: {ex.Message}";
            }

            return View("Test");
        }
    }
}
