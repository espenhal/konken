using System.Web.Http;
using AutoMapper;

namespace WebApi.Controllers
{
    public class BaseApiController : ApiController
    {
        public IMapper Mapper { get; set; }

        public BaseApiController()
        {
        }
    }
}