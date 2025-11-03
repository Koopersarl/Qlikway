using FtpBulkInsert.Models;
using FtpBulkInsert.Services;
using Microsoft.AspNetCore.Mvc;

namespace FtpBulkInsert.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IBulkInsertService _bulkInsertService;
        private readonly ILogService _logService;
        private readonly ILogger<UploadController> _logger;
        private readonly string _tempFolder;

        public UploadController(
            IBulkInsertService bulkInsertService,
            ILogService logService,
            IConfiguration configuration,
            ILogger<UploadController> logger)
        {
            _bulkInsertService = bulkInsertService;
            _logService = logService;
            _logger = logger;
            _tempFolder = configuration["TempFolder"] ?? Path.Combine(Path.GetTempPath(), "FtpBulkInsert");

            // Créer le dossier temporaire s'il n'existe pas
            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }
        }

        /// <summary>
        /// Upload manuel d'un ou plusieurs fichiers CSV
        /// </summary>
        [HttpPost("manual")]
        public async Task<ActionResult<ProcessResult>> UploadFiles([FromForm] IFormFileCollection files)
        {
            var processResult = new ProcessResult();

            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { message = "Aucun fichier fourni" });
                }

                _logger.LogInformation($"Upload manuel de {files.Count} fichier(s)");

                foreach (var file in files)
                {
                    if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning($"Fichier ignoré (pas un CSV): {file.FileName}");
                        continue;
                    }

                    try
                    {
                        _logger.LogInformation($"Traitement du fichier: {file.FileName}");

                        // Sauvegarder le fichier temporairement
                        var localPath = Path.Combine(_tempFolder, file.FileName);
                        using (var stream = new FileStream(localPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _logger.LogInformation($"Fichier sauvegardé: {localPath}");

                        // Effectuer le BULK INSERT
                        var result = await _bulkInsertService.ProcessFileAsync(file.FileName, localPath);

                        // Enregistrer le log
                        await _logService.LogUploadAsync(result);

                        // Ajouter aux résultats
                        processResult.Results.Add(result);

                        if (result.Success)
                        {
                            processResult.TotalRowsInserted += result.RowsInserted;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erreur lors du traitement du fichier {file.FileName}");

                        var errorResult = new BulkInsertResult
                        {
                            FileName = file.FileName,
                            TableName = Path.GetFileNameWithoutExtension(file.FileName),
                            Success = false,
                            ErrorMessage = ex.Message
                        };

                        processResult.Results.Add(errorResult);
                        await _logService.LogUploadAsync(errorResult);
                    }
                }

                processResult.TotalFilesProcessed = processResult.Results.Count;
                processResult.Success = processResult.Results.Any(r => r.Success);

                _logger.LogInformation($"Upload manuel terminé: {processResult.TotalFilesProcessed} fichiers, {processResult.TotalRowsInserted} lignes insérées");

                return Ok(processResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur globale lors de l'upload manuel");
                processResult.Success = false;
                processResult.ErrorMessage = ex.Message;
                return StatusCode(500, processResult);
            }
        }

        /// <summary>
        /// Récupérer les logs récents
        /// </summary>
        [HttpGet("logs")]
        public async Task<ActionResult<List<UploadLog>>> GetLogs([FromQuery] int count = 50)
        {
            try
            {
                var logs = await _logService.GetRecentLogsAsync(count);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des logs");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
