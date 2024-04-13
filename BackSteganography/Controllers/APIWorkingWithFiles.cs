using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace BackSteganography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIWorkingWithFiles : ControllerBase
    {

        private async Task<string> WriteFile(String uniqueUserID, IFormFile file, string folder)
        {
            try
            {
                string clientDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Upload", uniqueUserID);

                // Если передан верный токен, то папка должна уже существовать
                if (!Directory.Exists(clientDirectory))
                {
                    return "invalid_token";
                }

                string filepath = Path.Combine(clientDirectory, folder);

                Directory.CreateDirectory(filepath);

                // Если в папке уже существуют какие-то файлы, то удаляем их
                string[] files = Directory.GetFiles(filepath);
                if (files.Length > 0)
                {
                    foreach (string oldfile in files)
                    {
                        FileInfo fileInfo = new FileInfo(oldfile);
                        fileInfo.Delete();
                    }
                }

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                string exactpath = Path.Combine(filepath, file.FileName);
                using (var stream = new FileStream(exactpath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return uniqueUserID;
            }
            catch
            {
                return "error_when_trying_to_write_data_to_the_server";
            }


        }

        [HttpPost]
        [Route("UploadVideoFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status501NotImplemented)]

        public async Task<IActionResult> UploadVideoFile(String uniqueUserID, IFormFile file, CancellationToken cancellationtoken)
        {

            if (!CheckingTheFileFormat(file.FileName, new List<string> { ".mp4", ".avi", ".mkv" }))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "The file format is not supported.");
            }


            try
            {
                var result = await WriteFile(uniqueUserID, file, "Videos");
                if (result == null || result == "error_when_trying_to_write_data_to_the_server")
                {
                    return StatusCode(501, result);
                }
                else if (result == "invalid_token")
                {
                    return StatusCode(400, result);
                }
                else
                {
                    return StatusCode(200, result);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("UploadDataFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status501NotImplemented)]

        public async Task<IActionResult> UploadDataFile(String uniqueUserID, IFormFile file, CancellationToken cancellationtoken)
        {

            if (!CheckingTheFileFormat(file.FileName, new List<string> { ".txt" }))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "The file format is not supported.");
            }

            try
            {
                var result = await WriteFile(uniqueUserID, file, "DataFiles");
                if (result == null || result == "error_when_trying_to_write_data_to_the_server")
                {
                    return StatusCode(501, result);
                }
                else if (result == "invalid_token")
                {
                    return StatusCode(400, result);
                }
                else
                {
                    return StatusCode(200, result);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("DownloadFile")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> DownloadFile(String uniqueUserID, string filename)
        {
            try
            {
                string clientDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Upload", uniqueUserID);
                string filepath = Path.Combine(clientDirectory, "AlgorithmResult", filename);

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(filename, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(filepath);
                Directory.Delete(clientDirectory, true);
                return File(bytes, contentType, Path.GetFileName(filepath));
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("NewTestUploadingFileInChunks")]
        public async Task<IActionResult> UploadChunkV2(int chunkNumber, int totalChunks, string fileId, IFormFile chunkfile)
        {
            try
            {
                // Проверка на размер чанка
                // Хэш сумма чанка
                // Вернуть в процентах загруженные чанки

                string uploadPath = Path.Combine("Upload\\NewDataFilesTest\\", fileId);
                string chunkFilePath = Path.Combine(uploadPath, $"{chunkNumber}.part");

                using (var stream = new FileStream(chunkFilePath, FileMode.Create))
                {
                    await chunkfile.CopyToAsync(stream);
                }

                if (chunkNumber == totalChunks)
                {
                    // Combine all chunks to create the final file
                    await CombineChunksAsync(uploadPath, fileId, totalChunks);
                    // Delete temporary chunk files
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        private async Task CombineChunksAsync(string uploadPath, string fileId, int totalChunks)
        {
            var finalFilePath = Path.Combine(uploadPath, fileId);
            using (var finalStream = new FileStream(finalFilePath, FileMode.Create))
            {
                for (int i = 1; i <= totalChunks; i++)
                {
                    var chunkFilePath = Path.Combine(uploadPath, $"{i}.part");
                    using (var chunkStream = new FileStream(chunkFilePath, FileMode.Open))
                    {
                        await chunkStream.CopyToAsync(finalStream);
                    }
                }
            }
        }

        private void DeleteChunks(string uploadPath, int totalChunks)
        {
            for (int i = 1; i <= totalChunks; i++)
            {
                var chunkFilePath = Path.Combine(uploadPath, $"{i}.part");
                System.IO.File.Delete(chunkFilePath);
            }
        }

        bool CheckingTheFileFormat(string filename, List<string> supportedFileFormats)
        {

            string fileExtension = Path.GetExtension(filename);

            return supportedFileFormats.Contains(fileExtension);
        }
    }
}
