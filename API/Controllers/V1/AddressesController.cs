using API.Classes;
using API.Configuration;
using API.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers.V1 {
    /// <summary>
    /// Represents a RESTful service of addresses
    /// IMPORTANT - [Produces("application/json")] is required on every HTTP action or Swagger will not show what object model will be returned
    /// </summary>
    [ApiVersion("1.0", Deprecated = true)]
    [ODataRoutePrefix("addresses")]
    public class AddressesController : ODataController {
        private readonly ApiDbContext _db;
        private readonly TelemetryTracker _telemetryTracker;

        public AddressesController(ApiDbContext context, TelemetryTracker telemetryTracker) {
            _db = context;
            _telemetryTracker=telemetryTracker;
        }

        #region CRUD Operations

        /// <summary>Query addresses</summary>
        /// <returns>A list of addresses</returns>
        /// <response code="200">The addresses were successfully retrieved</response>
        [HttpGet]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
        [EnableQuery]
        [Authorize]
        public IActionResult Get() {
            try {
                return Ok(_db.Addresses.AsNoTracking());
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Query addresses by id</summary>
        /// <param name="id">The address id</param>
        /// <returns>A single address</returns>
        /// <response code="200">The address was successfully retrieved</response>
        /// <response code="404">The address was not found</response>
        [HttpGet]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select | AllowedQueryOptions.Expand)]
        [Authorize]
        public IActionResult GetById([FromRoute] int id) {
            try {
                //OData will handle returning 404 Not Found if IQueriable returns no result
                return Ok(SingleResult.Create(_db.Addresses.Where(e => e.Id == id).AsNoTracking()));
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Create a new address</summary>
        /// <param name="address">A full address object</param>
        /// <returns>A new address</returns>
        /// <response code="201">The address was successfully created</response>
        /// <response code="400">The address is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        [HttpPost]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Address), 201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [Authorize]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] Address address) {
            try {
                address.CreatedDate=DateTime.UtcNow;
                address.CreatedBy=User.Identity.Name ?? "Anonymous";
                address.LastModifiedDate=DateTime.UtcNow;
                address.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }
                _db.Addresses.Add(address);
                await _db.SaveChangesAsync();
                return Created("", address);
            } catch (Exception ex) {
                if (ex.InnerException.Message.Contains(Constants.errorSqlDuplicateKey)) {
                    return Conflict(Constants.messageDupEntity+Constants.messageAppInsights);
                } else {
                    return StatusCode(500, ex.Message+Constants.messageAppInsights);
                }
            }
        }

        /// <summary>Edit address</summary>
        /// <param name="id">The address id</param>
        /// <param name="addressDelta">A partial address object.  Only properties supplied will be updated.</param>
        /// <returns>An updated address</returns>
        /// <response code="200">The address was successfully updated</response>
        /// <response code="400">The address is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The address was not found</response>
        [HttpPatch]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Address), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized - User not authenticated
        [ProducesResponseType(typeof(ForbiddenException), 403)] // Forbidden - User does not have required claim roles
        [ProducesResponseType(typeof(void), 404)]
        [Authorize]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] Delta<Address> addressDelta) {
            try {
                var address = await _db.Addresses.FindAsync(id);
                if (address == null) {
                    return NotFound();
                }
                address.LastModifiedDate = DateTime.UtcNow;
                address.LastModifiedBy = User.Identity.Name ?? "Anonymous";
                addressDelta.Patch(address);
                await _db.SaveChangesAsync();
                return Ok(address);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Delete the given address</summary>
        /// <param name="id">The address id</param>
        /// <response code="204">The address was successfully deleted</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The address was not found</response>
        [HttpDelete]
        [ODataRoute("({id})")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized - User not authenticated
        [ProducesResponseType(typeof(ForbiddenException), 403)] // Forbidden - User does not have required claim roles
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [Authorize]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id) {
            try {
                Address address = await _db.Addresses.FindAsync(id);
                if (address == null) {
                    return NotFound();
                }
                _db.Addresses.Remove(address);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                if (ex.InnerException.InnerException.Message.Contains(Constants.errorSqlReferenceConflict)) {
                    // Reset the remove    
                    foreach (EntityEntry entityEntry in _db.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted)) {
                        entityEntry.State = EntityState.Unchanged;
                    }
                    _telemetryTracker.TrackException(ex);
                    return StatusCode(409, Constants.messageerrorSqlForeignKey + Constants.messageAppInsights);
                } else {
                    _telemetryTracker.TrackException(ex);
                    return StatusCode(500, ex.Message + Constants.messageAppInsights);
                }
            }
        }

    }

    #endregion

}
