using Babblr.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Babblr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly IStorageService _storageService;
    private static readonly string[] AllowedTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf"];

    public UploadsController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File size cannot exceed 10MB." });

        if (!AllowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "File type not allowed." });

        using var stream = file.OpenReadStream();
        var url = await _storageService.UploadFileAsync(
            stream, file.FileName, file.ContentType);

        return Ok(new { url, fileName = file.FileName, size = file.Length });
    }
}