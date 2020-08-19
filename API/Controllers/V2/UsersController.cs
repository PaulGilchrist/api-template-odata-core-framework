using API.Classes;
using API.Configuration;
using API.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable S125 // Sections of code should not be commented out
/*   
*  Example on how to get a string[] of roles from the user's token 
*      var roles = User.Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).FirstOrDefault().Value.Split(',');
*/

namespace API.Controllers.V2 {
    /// <summary>
    /// Represents a RESTful service of users
    /// IMPORTANT - [Produces("application/json")] is required on every HTTP action or Swagger will not show what object model will be returned
    /// </summary>
    [ApiVersion("2.0")]
    [ODataRoutePrefix("users")]
    public class UsersController : ODataController {
        private readonly ApiDbContext _db;
        private readonly TelemetryTracker _telemetryTracker;

        public UsersController(ApiDbContext context, TelemetryTracker telemetryTracker) {
            _db = context;
            _telemetryTracker = telemetryTracker;
        }

        #region CRUD Operations

        /// <summary>Query users</summary>
        /// <returns>A list of users</returns>
        /// <response code="200">The users were successfully retrieved</response>
        [HttpGet]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [EnableQuery]
        public IActionResult Get() {
            try {
                return Ok(_db.Users.AsNoTracking());
            } catch(Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Query users by id</summary>
        /// <param name="id">The user id</param>
        /// <returns>A single user</returns>
        /// <response code="200">The user was successfully retrieved</response>
        /// <response code="404">The user was not found</response>
        [HttpGet]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [EnableQuery]
        public IActionResult GetById([FromRoute] int id) {
            try {
                //OData will handle returning 404 Not Found if IQueriable returns no result
                return Ok(SingleResult.Create(_db.Users.Where(e => e.Id==id).AsNoTracking()));
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Create a new user</summary>
        /// <remarks>
        /// Supports either a single object or an array
        /// </remarks>
        /// <param name="user">A full user object</param>
        /// <returns>A new user or list of users</returns>
        /// <response code="201">The user was successfully created</response>
        /// <response code="400">The user is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        [HttpPost]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<User>), 201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] User user) {
            try {
                Request.Body.Position = 0;
                var streamReader = (await new StreamReader(Request.Body).ReadToEndAsync()).Trim();
                // Determine if a single object or an array was passed in
                if (streamReader.StartsWith("{")) {
                    var node = JsonConvert.DeserializeObject<User>(streamReader);
                    node.CreatedDate=DateTime.UtcNow;
                    node.CreatedBy=User.Identity.Name ?? "Anonymous";
                    node.LastModifiedDate=DateTime.UtcNow;
                    node.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                    ModelState.Clear();
                    TryValidateModel(node);
                    if (!ModelState.IsValid) {
                        return BadRequest(ModelState);
                    }
                    _db.Users.Add(node);
                    await _db.SaveChangesAsync();
                    return Created("", node);
                } else if (streamReader.StartsWith("[")) {
                    var nodes = JsonConvert.DeserializeObject<IEnumerable<User>>(streamReader);
                    foreach (var node in nodes) {
                        node.CreatedDate=DateTime.UtcNow;
                        node.CreatedBy=User.Identity.Name ?? "Anonymous";
                        node.LastModifiedDate=DateTime.UtcNow;
                        node.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                        ModelState.Clear();
                        TryValidateModel(node);
                        if (!ModelState.IsValid) {
                            return BadRequest(ModelState);
                        }
                        _db.Users.Add(node);
                    }
                    await _db.SaveChangesAsync();
                    return Created("", nodes);
                } else {
                    return BadRequest();
                }
            } catch (Exception ex) {
                if (ex.InnerException.Message.Contains(Constants.errorSqlDuplicateKey)) {
                    return Conflict(Constants.messageDupEntity+Constants.messageAppInsights);
                } else {
                    return StatusCode(500, ex.Message+Constants.messageAppInsights);
                }
            }
        }

        /// <summary>Edit user</summary>
        /// <param name="id">The user id</param>
        /// <param name="userDelta">A partial user object.  Only properties supplied will be updated.</param>
        /// <returns>An updated user</returns>
        /// <response code="200">The user was successfully updated</response>
        /// <response code="400">The user is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The user was not found</response>
        [HttpPatch]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized - User not authenticated
        [ProducesResponseType(typeof(ForbiddenException), 403)] // Forbidden - User does not have required claim roles
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] Delta<User> userDelta) {
            try {
                var user = await _db.Users.FindAsync(id);
                if (user==null) {
                    return NotFound();
                }
                user.LastModifiedDate=DateTime.UtcNow;
                user.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                userDelta.Patch(user);
                await _db.SaveChangesAsync();
                return Ok(user);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Bulk edit users</summary>
        /// <param name="userDeltas">An array of partial user objects.  Only properties supplied will be updated.</param>
        /// <returns>An updated list of users</returns>
        /// <response code="200">The user was successfully updated</response>
        /// <response code="400">The user is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The user was not found</response>
        [HttpPatch]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized - User not authenticated
        [ProducesResponseType(typeof(ForbiddenException), 403)] // Forbidden - User does not have required claim roles
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> PatchBulk([FromBody] IEnumerable<User> userDeltas) {
            try {
                Request.Body.Position = 0;
                var patchUsers = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(await new StreamReader(Request.Body).ReadToEndAsync());
                var userProperties = typeof(User).GetProperties();
                // Get list of all passed in Ids
                var idList = new List<int>();
                foreach (var patchUser in patchUsers) {
                    idList.Add((int)patchUser["id"]);
                }
                _db.ChangeTracker.AutoDetectChangesEnabled = false;
                // Make one SQL call to get all the database objects to Patch
                var users = await _db.Users.Where(st => idList.Contains(st.Id)).ToListAsync();
                // Update each database objects
                foreach (var patchUser in patchUsers) {
                    var user = users.Find(st => st.Id == (int)patchUser["id"]);
                    if (user== null) {
                        return NotFound();
                    }
                    foreach (var patchUserProperty in patchUser.Properties()) {
                        var patchUserPropertyName = patchUserProperty.Name;
                        if (patchUserPropertyName != "id") { // Cannot change the id but it will always be passed in
                            // Loop through the changed properties updating the object
                            for (int i = 0; i < userProperties.Length; i++) {
                                if (String.Equals(patchUserPropertyName, userProperties[i].Name, StringComparison.OrdinalIgnoreCase) == 0) {
                                    _db.Entry(user).Property(userProperties[i].Name).CurrentValue = Convert.ChangeType(patchUserProperty.Value, userProperties[i].PropertyType);
                                    _db.Entry(user).State = EntityState.Modified;
                                    break;
                                    // Could optionally even support deltas within deltas here
                                }
                            }
                        }
                    }
                    user.LastModifiedDate=DateTime.UtcNow;
                    user.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                    _db.Users.Update(user);
                }
                await _db.SaveChangesAsync();
                _db.ChangeTracker.AutoDetectChangesEnabled = true;
                return Ok(users);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Delete the given user</summary>
        /// <param name="id">The user id</param>
        /// <response code="204">The user was successfully deleted</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The user was not found</response>
        [HttpDelete]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id) {
            try {
                User user = await _db.Users.FindAsync(id);
                if (user == null) {
                    return NotFound();
                }
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                if (ex.InnerException.InnerException.Message.Contains(Constants.errorSqlReferenceConflict)) {
                    // Reset the remove    
                    foreach (EntityEntry entityEntry in _db.ChangeTracker.Entries().Where(e => e.State==EntityState.Deleted)) {
                        entityEntry.State=EntityState.Unchanged;
                    }
                    _telemetryTracker.TrackException(ex);
                    return StatusCode(409, Constants.messageerrorSqlForeignKey+Constants.messageAppInsights);
                } else {
                    _telemetryTracker.TrackException(ex);
                    return StatusCode(500, ex.Message+Constants.messageAppInsights);
                }
            }
        }

        #endregion

        #region REFs

        /// <summary>Associate an addresses to the user with the given id</summary>
        /// <param name="id">The user id</param>
        /// <param name="reference">The Uri of the address being associated.  Ex: {"@odata.id":"http://api.company.com/odata/address(1)"}</param>
        /// <response code="204">The address was successfully associated</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The address or user was not found</response>
        [HttpPost]
        [ODataRoute("({id})/addresses/$ref")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> PostAddressRef([FromRoute] int id, [FromBody] ODataReference reference) {
            try {
                User user = await _db.Users.FindAsync(id);
                if (user==null) {
                    return NotFound();
                }
                var addressId = Convert.ToInt32(ReferenceHelper.GetKeyFromUrl(reference.uri));
                Address address = await _db.Addresses.FindAsync(addressId);
                if (address==null) {
                    return NotFound();
                }
                user.Addresses.Add(address);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                if (ex.InnerException.Message.Contains(Constants.errorSqlDuplicateKey)) {
                    return Conflict(Constants.messageDupAssoc+Constants.messageAppInsights);
                } else {
                    return StatusCode(500, ex.Message+Constants.messageAppInsights);
                }
            }
        }

        /// <summary>Remove an address association from the user with the given id</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="userId">The user id</param>
        /// <param name="id">The Uri of the address association being removed.  Ex: id=http://api.company.com/odata/address(1)</param>
        /// <response code="204">The address association was successfully removed</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The address association was not found</response>
        /// <response code="409">SQL Conflict</response>
        [HttpDelete]
        [ODataRoute("({userId})/addresses/$ref")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [ProducesResponseType(typeof(string), 409)] // Conflict
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> DeleteAddressRef([FromRoute] int userId, [FromQuery] Uri id) {
            // This meets the spec, but so does having Uri in [FromBody] so it does not have to use the variable "id" I would prefer to use instead of userId
            try {
                User user = await _db.Users.Include(e => e.Addresses).FirstOrDefaultAsync(e => e.Id==userId);
                if (user==null) {
                    return NotFound();
                }
                var addressId = Convert.ToInt32(ReferenceHelper.GetKeyFromUrl(id));
                Address address = user.Addresses.First(e => e.Id==addressId);
                if (address==null) {
                    return NotFound();
                }
                user.Addresses.Remove(address);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message+Constants.messageAppInsights);
            }
        }

        /// <summary>Get the addresses for the user with the given id</summary>
        /// <param name="id">The user id</param>
        /// <returns>A list of addresses</returns>
        /// <response code="200">The addresses were successfully retrieved</response>
        [HttpGet]
        [ODataRoute("({id})/addresses")]
        [ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        public async Task<IActionResult> GetAddresses([FromRoute] int id) {
            try {
                var addresses = _db.Users.Where(e => e.Id==id).SelectMany(e => e.Addresses);
                if (!await addresses.AnyAsync()) {
                    return NotFound();
                }
                return Ok(addresses);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message+Constants.messageAppInsights);
            }
        }

        /// <summary>Query user notes</summary>
        /// <returns>A list of notes</returns>
        /// <response code="200">The notes were successfully retrieved</response>
        [HttpGet]
        [ODataRoute("({id})/notes")]
        [ProducesResponseType(typeof(IEnumerable<UserNote>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        public async Task<IActionResult> GetNotes([FromRoute] int id) {
            try {
                var notes = _db.UserNotes;
                if (!await notes.AnyAsync(n => n.User.Id==id)) {
                    return NotFound();
                }
                return Ok(notes);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message+Constants.messageAppInsights);
            }
        }

    }

    #endregion

}
