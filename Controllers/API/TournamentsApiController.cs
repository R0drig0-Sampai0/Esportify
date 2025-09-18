using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Esportify.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TournamentsController : ControllerBase
    {
        private readonly EsportifyContext _context;

        public TournamentsController(EsportifyContext context)
        {
            _context = context;
        }

    }
}