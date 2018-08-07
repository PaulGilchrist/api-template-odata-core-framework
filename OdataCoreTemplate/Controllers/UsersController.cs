using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using NSwag.Annotations;
using OdataCoreTemplate.Data;
using OdataCoreTemplate.Models;
using ODataCoreTemplate.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Produces("application/json")]
public class UsersController : ODataController {
    private ApiContext _db;

    public UsersController(ApiContext context) {
        _db = context;
        if (context.Users.Count() == 0) {
            foreach (var b in DataSource.GetUsers()) {
                context.Users.Add(b);
            }
            context.SaveChanges();
        }
    }

    // GET: odata/users
    // GET: odata/users?$expand=addresses
    // GET: odata/users?$expand=addresses($select= id)
    // GET: odata/users?$expand=addresses($expand=users($select=firstName,lastName))
    // GET: odata/users?$expand=addresses($expand=users($select=firstName,lastName;$orderby=firstName,lastName))
    // GET: odata/users?$filter(firstName eq 'Adam')
    // GET: odata/users?$orderby=firstName asc,lastName  //asc is the default, but just shown for this example.  Alternate sort is desc
    // GET: odata/users?$select=firstName,lastName
    /// <summary>Query users</summary>
    /// <response code="200">Ok</response>
    /// <response code="404">Not Found</response>	[HttpGet]
    [EnableQuery]
    [HttpGet]
    [ODataRoute("users")]
    //[SwaggerResponseAttribute(typeof(IEnumerable<User>))]

    public async Task<IActionResult> Get() {
        var users = _db.Users;
        if (!await users.AnyAsync()) {
            return NotFound();
        }
        return Ok(users);
    }

    // GET: odata/users(5)
    /// <summary>Query users by id</summary>
    /// <param name="id">The user id</param>
    /// <response code="200">Ok</response>
    /// <response code="404">Not Found</response>
    [EnableQuery]
    [HttpGet]
    [ODataRoute("users({id})")]
    //[SwaggerResponseAttribute(typeof(IEnumerable<User>))]
    public async Task<IActionResult> GetUser([FromODataUri] int id) {
        User user = await _db.Users.FindAsync(id);
        if (user == null) {
            return NotFound();
        }
        return Ok(user);
    }

    // POST: odata/users
    /// <summary>Create a new user</summary>
    /// <param name="user">A full user object.  Every property will be updated except id.</param>
    /// <response code="201">Created</response>
    /// <response code="400">Bad Request</response>
    [EnableQuery]
    [HttpPost]
    [ODataRoute("users")]
    //[SwaggerResponseAttribute(typeof(void))]
    public async Task<IActionResult> Post([FromBody] User user) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Created(user);
    }


}
