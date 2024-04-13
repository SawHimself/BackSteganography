using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackSteganography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIEncodingDecoding : ControllerBase
    {
        private string StartSecretAlgorithm(string filename)
        {
            // application logic
            return "Completed successfully! ResultFileName";
        }

        [HttpPut]
        [Route("StartDataEncoding")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartDataEncoding(string filename)
        {
            try
            {
                var result = StartSecretAlgorithm(filename);

                // Для отладки
                Thread.Sleep(1000);

                for (int i = 0; i < 20; i++)
                {
                    Console.WriteLine($"Ожидание {i + 1} секунды...");
                    Thread.Sleep(1000); // Остановка на одну секунду
                }

                Console.WriteLine("Ожидание завершено.");
                return Ok(result);
            }
            catch
            {
                return BadRequest("FileNotFount");
            }
        }

        [HttpPut]
        [Route("StartDataDecoding")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartDataDecoding(string filename)
        {
            return StatusCode(200);
        }
    }
}
