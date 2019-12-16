using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeagueController : ControllerBase
    {
        [HttpGet("{leagueId:Guid}")]
        public async Task<IActionResult> Get(Guid leagueId)
        {
            var league = "{\"yolo\": \"yeah!\"}";
            
            return Ok(league);
        }
    }
}