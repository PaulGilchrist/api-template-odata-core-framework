using API.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ODataCoreTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

/*   
*  Example on how to get an string[] of roles from the user's token 
*      var roles = User.Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).FirstOrDefault().Value.Split(',');
*/

namespace ODataCoreTemplate.Controllers.V2 {
    [ApiVersion("2.0")]
    [ODataRoutePrefix("users")]
    public class UsersController : ODataController {
        private OdataCoreTemplate.Models.ApiDbContext _db;

        public UsersController(OdataCoreTemplate.Models.ApiDbContext context) {
            _db = context;
        }


        /// <summary>Query users</summary>
        [HttpGet]
        [ODataRoute("")]
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
        [ODataRoute("({id})")]
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
        [ODataRoute("")]
        [ProducesResponseType(typeof(IEnumerable<User>), 201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[Authorize]
        public async Task<IActionResult> Post([FromBody] UserList userList) {
            // Swagger will give error if not using options.CustomSchemaIds((x) => x.Name + "_" + Guid.NewGuid());
            /*
            *   Other things tried were as follows:
            *   Native array - Not accepted by OData (input will be null)
            *   JArray - Accepted by OData, but Swagger will not show proper [FromBopdy] object
            *   ComplexTypeConfiguration<UserList> userList = builder.ComplexType<UserList>(); // Only works for POST
            *   builder.Action("users").ReturnsCollectionFromEntitySet<User>("users").Parameter<UserList>("userList"); // Only works for POST
            */
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var users = userList.value;
            foreach (User user in users) {
                _db.Users.Add(user);
            }
            await _db.SaveChangesAsync();
            return Created("", users);
        }

        /// <summary>Bulk edit users</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="userList">An object containing an array of partial user objects.
        /// See GET action for object model.  Since PATCH only requires the id property and those properties being modified, it does not have its own model
        /// </param>
        [HttpPatch]
        [ODataRoute("")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> Patch([FromBody] DynamicList userList) {
            var patchUsers = userList.value;
            List<User> dbUsers = new List<User>(0);
            System.Reflection.PropertyInfo[] userProperties = typeof(User).GetProperties();
            foreach (JObject patchUser in patchUsers) {
                var dbUser = _db.Users.Find((int)patchUser["id"]);
                if (dbUser == null) {
                    return NotFound();
                }
                var patchUseProperties = patchUser.Properties();
                foreach (var patchUserProperty in patchUseProperties) {
                    foreach (var userProperty in userProperties) {
                        if(String.Compare(patchUserProperty.Name, userProperty.Name, true)==0) {
                            _db.Entry(dbUser).Property(userProperty.Name).CurrentValue = Convert.ChangeType(patchUserProperty.Value, userProperty.PropertyType);
                        }
                    }
                }
                _db.Entry(dbUser).State = EntityState.Detached;
                _db.Users.Update(dbUser);
                dbUsers.Add(dbUser);
            }
            await _db.SaveChangesAsync();
            return Ok(dbUsers);
        }

        ///// <summary>Bulk edit users</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="deltaUserList">An object containing an array of partial user objects.  Only properties supplied will be updated.</param>
        //[HttpPatch]
        //[ODataRoute("")]
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
        //        var instance = userDelta.GetInstance();
        //        var dbUser = _db.Users.Find(instance.Id);
        //        if (dbUser == null) {
        //            return NotFound();
        //        }
        //        _db.Entry(dbUser).State = EntityState.Detached;
        //        userDelta.Patch(dbUser);
        //        dbUsers.Append(dbUser);
        //    }
        //    await _db.SaveChangesAsync();
        //    return Ok(dbUsers);
        //}

        /// <summary>Replace all data for an array of users</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="userList">An object containing an array of full user objects.  Every property will be updated except id.</param>
        [HttpPut]
        [ODataRoute("")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> Put([FromBody] UserList userList) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var users = userList.value;
            foreach (User user in users) {
                User dbUser = await _db.Users.FindAsync(user.Id);
                if (dbUser == null) {
                    return NotFound();
                }
                _db.Entry(dbUser).State = EntityState.Detached;
                _db.Users.Update(user);
            }
            await _db.SaveChangesAsync();
            return Ok(users);
        }

        /// <summary>Delete the given user</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The user id</param>
        [HttpDelete]
        [ODataRoute("({id})")]
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

        /// <summary>Get the addresses for the user with the given id</summary>
        /// <param name="id">The user id</param>
        [HttpGet]
        [ODataRoute("({id})/addresses")]
        [ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        public IQueryable<Address> GetAddresses([FromRoute] int id) {
            return _db.Users.Where(m => m.Id == id).SelectMany(m => m.Addresses);
        }

        /// <summary>Associate an addresses to the user with the given id</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The user id</param>
        /// <param name="addressId">The address id to associate with the user</param>
        [HttpPost]
        [ODataRoute("({id})/addresses({addressId})")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> LinkAddresses([FromRoute] int id, [FromRoute] int addressId) {
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
        [ODataRoute("({id})/addresses({addressId})")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        // [Authorize]
        public async Task<IActionResult> UnlinkAddresses([FromRoute] int id, [FromRoute] int addressId) {
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
}
