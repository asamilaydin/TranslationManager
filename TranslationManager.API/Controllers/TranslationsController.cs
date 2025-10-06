using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TranslationManager.API.Interfaces;
using TranslationManager.API.Models;

namespace TranslationManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationsController : ControllerBase
    {
        private readonly ITranslationService _translationService;
        private readonly IImportService _importService;
        private readonly IExportService _exportService;
        private readonly ILogger<TranslationsController> _logger;

        public TranslationsController(
            ITranslationService translationService,
            IImportService importService,
            IExportService exportService,
            ILogger<TranslationsController> logger)
        {
            _translationService = translationService;
            _importService = importService;
            _exportService = exportService;
            _logger = logger;
        }

        // GET: api/translations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Translation>>> GetTranslations(
            [FromQuery] string? platform = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var translations = await _translationService.GetTranslationsAsync(platform, search);
                return Ok(translations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/translations/latest
        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<Translation>>> GetRecentTranslations()
        {
            try
            {
                var translations = await _translationService.GetRecentTranslationsAsync();
                return Ok(translations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/translations
        [HttpPost]
        public async Task<ActionResult<Translation>> PostTranslation(Translation translation)
        {
            try
            {
                var createdTranslation = await _translationService.CreateTranslationAsync(translation);
                return Ok(createdTranslation);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/translations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTranslation(int id, Translation translation)
        {
            try
            {
                if (id != translation.Id)
                {
                    return BadRequest("ID mismatch");
                }

                await _translationService.UpdateTranslationAsync(id, translation);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/translations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTranslation(int id)
        {
            try
            {
                await _translationService.DeleteTranslationAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/translations/selected
        [HttpDelete("selected")]
        public async Task<IActionResult> DeleteSelectedTranslations([FromBody] int[] ids)
        {
            try
            {
                await _translationService.DeleteSelectedTranslationsAsync(ids);
                return Ok($"Deleted {ids.Length} translations");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/translations/all
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllTranslations()
        {
            try
            {
                await _translationService.DeleteAllTranslationsAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/translations/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportTranslations([FromBody] LegacyImportRequest request)
        {
            try
            {
                var result = await _importService.ImportSingleAsync(request);
                return Ok(result);
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate key value violates unique constraint") || 
                                     (ex.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true))
            {
                return BadRequest("A translation with the same key already exists. Each translation key must be unique across all platforms.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/translations/import-bulk
        [HttpPost("import-bulk")]
        public async Task<IActionResult> ImportBulkTranslations([FromBody] BulkImportRequest request)
        {
            try
            {
                var result = await _importService.ImportBulkAsync(request);
                return Ok(result);
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate key value violates unique constraint") || 
                                     (ex.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true))
            {
                return BadRequest("A translation with the same key already exists. Each translation key must be unique across all platforms.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/translations/import-excel
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcelTranslations(IFormFile file, [FromForm] string? platform = null, [FromForm] bool? mobileSynced = null)
        {
            try
            {
                var result = await _importService.ImportExcelAsync(file, platform, mobileSynced);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate key value violates unique constraint") || 
                                     (ex.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true))
            {
                return BadRequest("A translation with the same key already exists. Each translation key must be unique across all platforms.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing Excel file: {ex.Message}");
            }
        }

        // GET: api/translations/export/{language}
        [HttpGet("export/{language}")]
        public async Task<ActionResult<object>> ExportTranslations(string language)
        {
            try
            {
                var result = await _exportService.ExportSingleLanguageAsync(language);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/translations/export-bulk
        [HttpGet("export-bulk")]
        public async Task<ActionResult<object>> ExportBulkTranslations()
        {
            try
            {
                var result = await _exportService.ExportBulkAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}