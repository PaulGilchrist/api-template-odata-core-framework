﻿using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using OdataCoreTemplate.Classes;
using OdataCoreTemplate.Models;
using ODataCoreTemplate.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*   
*  Example on how to get an string[] of roles from the user's token 
*      var roles = User.Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).FirstOrDefault().Value.Split(',');
*/

[ODataController(typeof(User))]
public class UsersController : Controller {
    private OdataCoreTemplate.Models.ApiDbContext _db;

    public UsersController(OdataCoreTemplate.Models.ApiDbContext context) {
        _db = context;
    }

    /// <summary>Query users</summary>
    [HttpGet]
    [Route("odata/users")]
    [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
    [ProducesResponseType(typeof(void), 404)]  // Not Found
    [EnableQuery(MaxNodeCount = 100000)]
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
    [Route("odata/users({id})")]
    [ProducesResponseType(typeof(User), 200)] // Ok
    [ProducesResponseType(typeof(void), 404)] // Not Found
    [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select | AllowedQueryOptions.Expand)]
    public async Task<IActionResult> GetUser(int id) {
        User user = await _db.Users.FindAsync(id);
        if (user == null) {
            return NotFound();
        }
        return Ok(user);
    }

    /// <summary>Create a new user</summary>
    /// <remarks>
    /// Make sure to secure this action before production release
    /// </remarks>
    /// <param name="user">A full user object</param>
    [HttpPost]
    [Route("odata/users")]
    [ProducesResponseType(typeof(User), 201)] // Created
    [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
    [ProducesResponseType(typeof(void), 401)] // Unauthorized
    //[Authorize]
    public async Task<IActionResult> Post([FromBody] User user) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Created("", user);
    }

    /// <summary>Edit the user with the given id</summary>
    /// <remarks>
    /// Make sure to secure this action before production release
    /// </remarks>
    /// <param name="id">The user id</param>
    /// <param name="userDelta">A partial user object.  Only properties supplied will be updated.</param>
    [HttpPatch]
    [Route("odata/users({id})")]
    [ProducesResponseType(typeof(User), 200)] // Ok
    [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
    [ProducesResponseType(typeof(void), 401)] // Unauthorized
    [ProducesResponseType(typeof(void), 404)] // Not Found
    //[Authorize]
    public async Task<IActionResult> Patch(int id, [FromBody] Delta<User> userDelta) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }
        var dbUser = _db.Users.Find(id);
        if (dbUser == null) {
            return NotFound();
        }
        userDelta.Patch(dbUser);
        await _db.SaveChangesAsync();
        return Ok(dbUser);
    }

    /// <summary>Replace all data for the user with the given id</summary>
    /// <remarks>
    /// Make sure to secure this action before production release
    /// </remarks>
    /// <param name="id">The user id</param>
    /// <param name="user">A full user object.  Every property will be updated except id.</param>
    [HttpPut]
    [Route("odata/users({id})")]
    [ProducesResponseType(typeof(User), 200)] // Ok
    [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
    [ProducesResponseType(typeof(void), 401)] // Unauthorized
    [ProducesResponseType(typeof(void), 404)] // Not Found
    //[Authorize]
    public async Task<IActionResult> Put(int id, [FromBody] User user) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }
        User dbUser = await _db.Users.FindAsync(id);
        if (dbUser == null) {
            return NotFound();
        }
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    /// <summary>Delete the given user</summary>
    /// <remarks>
    /// Make sure to secure this action before production release
    /// </remarks>
    /// <param name="id">The user id</param>
    [HttpDelete]
    [Route("odata/users({id})")]
    [ProducesResponseType(typeof(void), 204)] // No Content
    [ProducesResponseType(typeof(void), 401)] // Unauthorized
    [ProducesResponseType(typeof(void), 404)] // Not Found
    //[Authorize]
    public async Task<IActionResult> Delete(int id) {
        User user = await _db.Users.FindAsync(id);
        if (user == null) {
            return NotFound();
        }
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }


    /// <summary>Get the addresses for the user with the given id</summary>
    /// <param name="id">The user id</param>
    [HttpGet]
    [Route("odata/users({id})/addresses")]
    [ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
    [ProducesResponseType(typeof(void), 404)]  // Not Found
    [EnableQuery]
    public IQueryable<Address> GetAddresses(int id) {
        return _db.Users.Where(m => m.Id == id).SelectMany(m => m.Addresses);
    }

    /// <summary>Associate an addresses to the user with the given id</summary>
    /// <remarks>
    /// Make sure to secure this action before production release
    /// </remarks>
    /// <param name="id">The user id</param>
    /// <param name="addressId">The address id to associate with the user</param>
    [HttpPost]
    [Route("odata/users({id})/addresses({addressId})")]
    [ProducesResponseType(typeof(void), 204)] // No Content
    [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
    [ProducesResponseType(typeof(void), 401)] // Unauthorized
    [ProducesResponseType(typeof(void), 404)] // Not Found
    //[Authorize]
    public async Task<IActionResult> LinkAddresses(int id, int addressId) {
        User user = await _db.Users.FindAsync(id);
        if (user == null) {
            return NotFound();
        }
        if (user.Addresses.Any(i => i.Id == addressId)) {
            return BadRequest(string.Format("The user with id {0} is already linked to the address with id {1}", id, addressId));
        }
        Address address = await _db.Addresses.FindAsync(addressId);
        if (address == null) {
            return NotFound();
        }
        user.Addresses.Add(address);
        await _db.SaveChangesAsync();
        return NoContent();
    }


    /// <summary>Remove an address association from the user with the given id</summary>
    /// <remarks>
    /// Make sure to secure this action before production release
    /// </remarks>
    /// <param name="id">The user id</param>
    /// <param name="addressId">The address id to remove association from the user</param>
    [HttpDelete]
    [Route("odata/users({id})/addresses({addressId})")]
    [ProducesResponseType(typeof(void), 204)] // No Content
    [ProducesResponseType(typeof(void), 401)] // Unauthorized
    [ProducesResponseType(typeof(void), 404)] // Not Found
    // [Authorize]
    public async Task<IActionResult> UnlinkAddresses(int id, int addressId) {
        User user = await _db.Users.FindAsync(id);
        if (user == null) {
            return NotFound();
        }
        Address address = await _db.Addresses.FindAsync(addressId);
        if (address == null) {
            return NotFound();
        }
        user.Addresses.Remove(address);
        await _db.SaveChangesAsync();
        return NoContent();
    }


}
