using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.TestFile;

[ApiController]
public class TestFile : ControllerBase
{
    public new class Request
    {
        public IFormFile? File { get; set; }
        public string FileName { get; set; } = default!;
    }

    [HttpPatch("file")]
    [Tags("Test")]
    [SwaggerOperation(
        Summary = "Test Upload File",
        Description = "This API is for test upload file"
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromForm] Request request, [FromServices] GoogleStorageService storageService)
    {

            string? avatarUrl = null;
            try
            {
                avatarUrl = await storageService.UploadFileToCloudStorage(request.File!, request.FileName);
            }
            catch (Exception)
            {
                if (avatarUrl != null)
                {
                    await storageService.DeleteFileFromCloudStorage(avatarUrl);
                }
            throw FPTPlaygroundException.NewBuilder()
            .WithCode(FPTPlaygroundErrorCode.FPS_00)
            .AddReason("token", "Missing Token")
            .Build();
        }

        return Ok($"Cập nhật thành công {avatarUrl}");
    }
}
