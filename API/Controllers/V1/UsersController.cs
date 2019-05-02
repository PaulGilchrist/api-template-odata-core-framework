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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/*   
*  Example on how to get a string[] of roles from the user's token 
*      var roles = User.Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).FirstOrDefault().Value.Split(',');
*/

namespace API.Controllers.V1 {
    [ApiVersion("1.0")]
    [ODataRoutePrefix("users")]
    public class UsersController : ODataController {
        private ApiDbContext _db;
        private TelemetryTracker _telemetryTracker;

        public UsersController(ApiDbContext context, TelemetryTracker telemetryTracker) {
            _db = context;
            _telemetryTracker = telemetryTracker;
        }

        #region CRUD Operations

        /// <summary>Query users</summary>
        [HttpGet]
        [ODataRoute("")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [EnableQuery]
        public IActionResult Get() {
            try {
                return Ok(_db.Users);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Query users by id</summary>
        /// <param name="id">The user id</param>
        [HttpGet]
        [ODataRoute("({id})")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [EnableQuery]
        public IActionResult GetById([FromRoute] int id) {
            try {
                //OData will handle returning 404 Not Found if IQueriable returns no result
                return Ok(SingleResult.Create(_db.Users.Where(e => e.Id==id)));
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Create a new user</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="user">A full user object</param>
        [HttpPost]
        [ODataRoute("")]
        [ProducesResponseType(typeof(User), 201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] User user) {
            try {
                user.CreatedDate=DateTime.UtcNow;
                user.CreatedBy=User.Identity.Name ?? "Anonymous";
                user.LastModifiedDate=DateTime.UtcNow;
                user.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return Created("", user);
            } catch (Exception ex) {
                if (ex.InnerException.Message.Contains(Constants.errorSqlDuplicateKey)) {
                    return Conflict(Constants.messageDupEntity+Constants.messageAppInsights);
                } else {
                    return StatusCode(500, ex.Message+Constants.messageAppInsights);
                }
            }
        }

        /// <summary>Edit user</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The user id</param>
        /// <param name="userDelta">A partial user object.  Only properties supplied will be updated.</param>
        [HttpPatch]
        [ODataRoute("({id})")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized - User not authenticated
        [ProducesResponseType(typeof(ForbiddenException), 403)] // Forbidden - User does not have required claim roles
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(Roles = "Admin")]
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

        /// <summary>Replace all data for a user</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The user id</param>
        /// <param name="user">A full user object.  Every property will be updated except id.</param>
        [HttpPut]
        [ODataRoute("({id})")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] User user) {
            try {
                user.Id=id;
                user.LastModifiedDate=DateTime.UtcNow;
                user.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
                return Ok(user);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                if(ex.InnerException.Message.Contains(Constants.errorSqlDoesNotExist)) {
                    return NotFound();
                } else {
                    return StatusCode(500, ex.Message + Constants.messageAppInsights);
                }
            }
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
        //[Authorize(Roles = "Admin")]
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
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The user id</param>
        /// <param name="reference">The Uri of the address being associated.  Ex: {"@odata.id":"http://api.company.com/odata/address(1)"}</param>
        [HttpPost]
        [ODataRoute("({id})/addresses/$ref")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(Roles = "Admin")]
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
        [HttpDelete]
        [ODataRoute("({userId})/addresses/$ref")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(Roles = "Admin")]
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
