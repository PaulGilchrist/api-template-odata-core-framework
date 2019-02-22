using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ODataCoreTemplate.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*   
*  Example on how to get an string[] of roles from the user's token 
*      var roles = User.Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).FirstOrDefault().Value.Split(',');
*/

namespace ODataCoreTemplate.V2 {
    [ApiVersion("2.0")]
    public class UsersController : ODataController {
        private OdataCoreTemplate.Models.ApiDbContext _db;

        public UsersController(OdataCoreTemplate.Models.ApiDbContext context) {
            _db = context;
        }


        /// <summary>Query users</summary>
        [HttpGet]
        [ODataRoute("users")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All, MaxNodeCount = 100000)]
        public async Task<IActionResult> Get() {
            var users = _db.Users;
            if (!await users.AnyAsync()) {
                return NotFound();
            }
            return Ok(users);
        }

        /// <summary>Query users by id</summary>
        /// <param name="id">The user id</param>
        [HttpGet]
        [ODataRoute("users({id})")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All)]
        public async Task<IActionResult> GetUser([FromRoute] int id) {
            User user = await _db.Users.FindAsync(id);
            if (user == null) {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>Create one or more new users</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="userList">An object containing an array of full user objects</param>
        [HttpPost]
        [ODataRoute("users")]
        [ProducesResponseType(typeof(IEnumerable<User>), 201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[Authorize]
        public async Task<IActionResult> Post([FromBody] UserList userList) {
            // Works for both OData and Swagger, but only for POST (not PUT or PATCH)
            //     Requires adding the following OData function
            //     builder.Function("users").Returns<IEnumerable<User>>().Parameter<UserList>("userList");
            var users = userList.value;
            foreach (User user in users) {
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }
                _db.Users.Add(user);
            }
            await _db.SaveChangesAsync();
            return Created("", users);
        }

        ///// <summary>Create one or more new users</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="userList">An object containing an array of full user objects</param>
        //[HttpPost]
        //[ODataRoute("users")]
        //[ProducesResponseType(typeof(IEnumerable<User>), 201)] // Created
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        //[ProducesResponseType(typeof(void), 401)] // Unauthorized
        ////[Authorize]
        //public async Task<IActionResult> Post([FromBody] UserList userList) {
        //    // OData call works, but Swagger gives error "Failed to Load API definition", "Fetch Error"
        //    var users = userList.value;
        //    foreach (User user in users) {
        //        if (!ModelState.IsValid) {
        //            return BadRequest(ModelState);
        //        }
        //        _db.Users.Add(user);
        //    }
        //    await _db.SaveChangesAsync();
        //    return Created("", users);
        //}

        ///// <summary>Create one or more new users</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="users">An array of full user objects</param>
        //[HttpPost]
        //[ODataRoute("users")]
        //[ProducesResponseType(typeof(IEnumerable<User>), 201)] // Created
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        //[ProducesResponseType(typeof(void), 401)] // Unauthorized
        ////[Authorize]
        //public async Task<IActionResult> Post([FromBody] IEnumerable<User> users) {
        //    //Proper Swagger but passes in null as JSON can not convert to array to single User object
        //    foreach (User user in users) {
        //        if (!ModelState.IsValid) {
        //            return BadRequest(ModelState);
        //        }
        //        _db.Users.Add(user);
        //    }
        //    await _db.SaveChangesAsync();
        //    return Created("", users);
        //}

        ///// <summary>Create one or more new users</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="users">An array of full user objects</param>
        //[HttpPost]
        //[ODataRoute("users")]
        //[ProducesResponseType(typeof(IEnumerable<User>), 201)] // Created
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        //[ProducesResponseType(typeof(void), 401)] // Unauthorized
        ////[Authorize]
        //public async Task<IActionResult> Post([FromBody] JArray users) {
        //    // Works but Swagger does not know the object type
        //    var userList = users.ToObject<List<User>>();
        //    foreach (User user in userList) {
        //        if (!ModelState.IsValid) {
        //            return BadRequest(ModelState);
        //        }
        //        _db.Users.Add(user);
        //    }
        //    await _db.SaveChangesAsync();
        //    return Created("", userList);
        //}

        ///// <summary>Bulk edit users</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="deltaUserList">An object containing an array of partial user objects.  Only properties supplied will be updated.</param>
        //[HttpPatch]
        //[ODataRoute("users")]
        //[ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        //[ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[ProducesResponseType(typeof(void), 404)] // Not Found
        ////[Authorize]
        //public async Task<IActionResult> Patch([FromBody] DeltaUserList deltaUserList) {
        //    var deltaUsers = deltaUserList.value;
        //    User[] dbUsers = new User[0];
        //    foreach (Delta<User> userDelta in deltaUsers) {
        //        if (!ModelState.IsValid) {
        //            return BadRequest(ModelState);
        //        }
        //        var dbUser = _db.Users.Find(userDelta.GetInstance().Id);
        //        if (dbUser == null) {
        //            return NotFound();
        //        }
        //        userDelta.Patch(dbUser);
        //        dbUsers.Append(dbUser);
        //    }
        //    await _db.SaveChangesAsync();
        //    return Ok(dbUsers);
        //}

        ///// <summary>Replace all data for an array of users</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="userList">An object containing an array of full user objects.  Every property will be updated except id.</param>
        //[HttpPut]
        //[ODataRoute("users")]
        //[ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        //[ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[ProducesResponseType(typeof(void), 404)] // Not Found
        ////[Authorize]
        //public async Task<IActionResult> Put([FromBody] UserList userList) {
        //    var users = userList.value;
        //    foreach (User user in users) {
        //        if (!ModelState.IsValid) {
        //            return BadRequest(ModelState);
        //        }
        //        User dbUser = await _db.Users.FindAsync(user.Id);
        //        if (dbUser == null) {
        //            return NotFound();
        //        }
        //        _db.Users.Update(user);
        //    }
        //    await _db.SaveChangesAsync();
        //    return Ok(users);
        //}

        /// <summary>Delete the given user</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The user id</param>
        [HttpDelete]
        [ODataRoute("users({id})")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id) {
            User user = await _db.Users.FindAsync(id);
            if (user == null) {
                return NotFound();
            }
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        ///// <summary>Get the addresses for the user with the given id</summary>
        ///// <param name="id">The user id</param>
        //[HttpGet]
        //[ODataRoute("({id})/addresses")]
        //[ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
        //[ProducesResponseType(typeof(void), 404)]  // Not Found
        //[EnableQuery]
        //public IQueryable<Address> GetAddresses([FromRoute] int id) {
        //    return _db.Users.Where(m => m.Id == id).SelectMany(m => m.Addresses);
        //}

        ///// <summary>Associate an addresses to the user with the given id</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="id">The user id</param>
        ///// <param name="addressId">The address id to associate with the user</param>
        //[HttpPost]
        //[ODataRoute("users({id})/addresses({addressId})")]
        //[ProducesResponseType(typeof(void), 204)] // No Content
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        //[ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[ProducesResponseType(typeof(void), 404)] // Not Found
        ////[Authorize]
        //public async Task<IActionResult> LinkAddresses([FromRoute] int id, [FromRoute] int addressId) {
        //    User user = await _db.Users.FindAsync(id);
        //    if (user == null) {
        //        return NotFound();
        //    }
        //    if (user.Addresses.Any(i => i.Id == addressId)) {
        //        return BadRequest(string.Format("The user with id {0} is already linked to the address with id {1}", id, addressId));
        //    }
        //    Address address = await _db.Addresses.FindAsync(addressId);
        //    if (address == null) {
        //        return NotFound();
        //    }
        //    user.Addresses.Add(address);
        //    await _db.SaveChangesAsync();
        //    return NoContent();
        //}


        ///// <summary>Remove an address association from the user with the given id</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="id">The user id</param>
        ///// <param name="addressId">The address id to remove association from the user</param>
        //[HttpDelete]
        //[ODataRoute("users({id})/addresses({addressId})")]
        //[ProducesResponseType(typeof(void), 204)] // No Content
        //[ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[ProducesResponseType(typeof(void), 404)] // Not Found
        //// [Authorize]
        //public async Task<IActionResult> UnlinkAddresses([FromRoute] int id, [FromRoute] int addressId) {
        //    User user = await _db.Users.FindAsync(id);
        //    if (user == null) {
        //        return NotFound();
        //    }
        //    Address address = await _db.Addresses.FindAsync(addressId);
        //    if (address == null) {
        //        return NotFound();
        //    }
        //    user.Addresses.Remove(address);
        //    await _db.SaveChangesAsync();
        //    return NoContent();
        //}

    }
}
