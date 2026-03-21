using Microsoft.AspNetCore.Mvc;

namespace Camp_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetCamps()
        {
            return Ok(new[]
            {
                new { Id = 1, Name = "Desert Camp", Price = 50 },
                new { Id = 2, Name = "Mountain Camp", Price = 80 }
            });
        }
    }
}
