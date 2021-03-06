using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataConnectContext;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApiDemo.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        public DataContext _context;
        public MessageController(DataContext dataContext)
        {
            _context = dataContext;

        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<Message>> Get()
        {
            //var data = new Message { clientuniqueid = "23", message = "hello adminxxx", date = DateTime.Now, Id = 11 };
            //var listResult = new List<Message>();
            //listResult.Add(data);
            //return listResult;
            var result = _context.Message.ToList();
            return result;
        }
    }
}
