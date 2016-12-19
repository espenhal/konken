using System.Web.Http;
using System.Web.Http.Cors;
using AutoMapper;

namespace WebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class BaseApiController : ApiController
    {
        public IMapper Mapper { get; set; }

        public BaseApiController()
        {
        }
    }
}