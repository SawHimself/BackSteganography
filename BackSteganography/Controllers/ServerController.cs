using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackSteganography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        [HttpGet]
        [Route("ServerStatusInfo")]
        public IActionResult GetServerInfo()
        {
            return Ok("Yes, I’m working, I’m working, leave me alone");
        }
        [HttpGet]
        [Route("GetSessionId")]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult InitializeSession()
        {
            try
            {
                string uniqueUserID = Guid.NewGuid().ToString();

                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", uniqueUserID);

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                return Ok(uniqueUserID);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("GetCurrentFiles")]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult GetSessionStatus(string uniqueUserID)
        {

            try
            {
                string clientDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Upload", uniqueUserID);
                string? videoFile = null;
                string? dataFile = null;

                if (!Directory.Exists(clientDirectory))
                {
                    return StatusCode(200, "session_not_found");
                }
                try
                {
                    string[] DataFiles = Directory.GetFiles(Path.Combine(clientDirectory, "DataFiles"));
                    dataFile = DataFiles[0].Replace(Path.Combine(clientDirectory, "DataFiles\\"), "");
                }
                catch { }

                try
                {
                    string[] VideoFiles = Directory.GetFiles(Path.Combine(clientDirectory, "Videos"));
                    videoFile = VideoFiles[0].Replace(Path.Combine(clientDirectory, "Videos\\"), "");
                }
                catch { }

                var CurrentFiles = new
                {
                    videoFile = videoFile,
                    dataFile = dataFile
                };

                string jsonRespons = JsonConvert.SerializeObject(CurrentFiles);

                return Ok(jsonRespons);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
