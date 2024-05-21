using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BackSteganography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIEncodingDecoding : ControllerBase
    {
        [HttpPut]
        [Route("StartDataEncoding")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult StartDataEncoding(string uniqueUserID, string datafilename, string videofilename)
        {
            try
            {
                uniqueUserID = Path.Combine(Directory.GetCurrentDirectory(), "Upload", uniqueUserID);
                if (!Directory.Exists(uniqueUserID))
                {
                    return StatusCode(400, "invalid_token");
                }

                datafilename = Path.Combine(uniqueUserID, "DataFiles", datafilename);

                FileInfo fileInfo = new FileInfo(datafilename);
                if (!fileInfo.Exists)
                {
                    return StatusCode(404, "data_file_not_found");
                }

                videofilename = Path.Combine(uniqueUserID, "Videos", videofilename);
                fileInfo = new FileInfo(videofilename);
                if (!fileInfo.Exists)
                {
                    return StatusCode(404, "video_file_not_found");
                }

                string resultDirectory = Path.Combine(uniqueUserID, "Result");

                string ProcessID = Guid.NewGuid().ToString();
                string ProcessLogFile = Path.Combine(resultDirectory, ProcessID);

                List<string> arguments = new List<string>();
                arguments.Add(videofilename);
                arguments.Add(datafilename);
                arguments.Add(resultDirectory);
                string encodeAlgorithm = Path.Combine("utilities", "encode.py");
                _ = Task.Run(() => StartPythonAlgorithm(encodeAlgorithm, ProcessLogFile, arguments));
                return StatusCode(200, ProcessID);
            }
            catch
            {
                return StatusCode(500, "The_encoding_utility_is_damaged_or_missing");
            }
        }

        [HttpPut]
        [Route("StartDataDecoding")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult StartDataDecoding(string uniqueUserID, string videofilename)
        {
            try
            {
                uniqueUserID = Path.Combine(Directory.GetCurrentDirectory(), "Upload", uniqueUserID);
                if (!Directory.Exists(uniqueUserID))
                {
                    return StatusCode(400, "invalid_token");
                }

                videofilename = Path.Combine(uniqueUserID, "Videos", videofilename);

                FileInfo fileInfo = new FileInfo(videofilename);
                if (!fileInfo.Exists)
                {
                    return StatusCode(404, "file_not_found");
                }

                string scriptName = Path.Combine(Directory.GetCurrentDirectory(), "utilities", "decode.py");
                string resultDirectory = Path.Combine(uniqueUserID, "Result");

                string ProcessID = Guid.NewGuid().ToString();
                string ProcessLogFile = Path.Combine(resultDirectory, ProcessID);

                List<string> arguments = new List<string>();
                arguments.Add(videofilename);
                arguments.Add(resultDirectory);
                string decodeAlgorithm = Path.Combine("utilities", "decode.py");
                _ = Task.Run(() => StartPythonAlgorithm(decodeAlgorithm, ProcessLogFile, arguments));
                return StatusCode(200, ProcessID);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("GetLastMessageInLogs")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LastMessageInLogs(string uniqueUserID, string ProcessID)
        {
            try
            {
                uniqueUserID = Path.Combine(Directory.GetCurrentDirectory(), "Upload", uniqueUserID);
                if (!Directory.Exists(uniqueUserID))
                {
                    return StatusCode(400, "invalid_token");
                }
                string logPath = Path.Combine(uniqueUserID, "Result", ProcessID);
                FileInfo fileInfo = new FileInfo(logPath);
                if (!fileInfo.Exists)
                {
                    return StatusCode(404, "log_file_not_found");
                }

                string lastLine = "";
                using (StreamReader reader = new StreamReader(logPath))
                {
                    string? buf;
                    while ((buf = await reader.ReadLineAsync()) != null)
                    {
                        lastLine = buf;
                    }
                }

                return StatusCode(200, lastLine);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        private async Task StartPythonAlgorithm(string scriptName, string ProcessLogFile, List<string> Arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "python3";

            psi.ArgumentList.Add(scriptName);

            foreach (string argument in Arguments)
            {
                psi.ArgumentList.Add(argument);
            }

            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            Process? process = Process.Start(psi);

            if (process != null)
            {
                StreamReader psr = process.StandardOutput;

                StringBuilder logOutTextBuilder = new StringBuilder();

                while (!psr.EndOfStream) // Read output line by line asynchronously
                {
                    string? line = await psr.ReadLineAsync();
                    if (line != null)
                    {
                        logOutTextBuilder.AppendLine(line);

                        using (StreamWriter writer = new StreamWriter(ProcessLogFile, true))
                        {
                            await writer.WriteLineAsync(line);
                        }
                    }
                }
            }
        }
    }
}
