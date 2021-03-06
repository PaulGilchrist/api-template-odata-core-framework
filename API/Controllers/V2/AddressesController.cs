﻿using API.Classes;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers.V2 {
    /// <summary>
    /// Represents a RESTful service of addresses
    /// IMPORTANT - [Produces("application/json")] is required on every HTTP action or Swagger will not show what object model will be returned
    /// </summary>
    [ApiVersion("2.0")]
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
                return Ok(SingleResult.Create(_db.Addresses.Where(e => e.Id==id).AsNoTracking()));
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Create a new address</summary>
        /// <remarks>
        /// Supports either a single object or an array
        /// </remarks>
        /// <param name="address">A full address object</param>
        /// <returns>A new address or list of addresses</returns>
        /// <response code="201">The address was successfully created</response>
        /// <response code="400">The address is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        [HttpPost]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Address>), 201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized - User not authenticated
        [ProducesResponseType(typeof(ForbiddenException), 403)] // Forbidden - User does not have required claim roles
        [Authorize]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] Address address) {
            try {
                Request.Body.Position = 0;
                var streamReader = (await new StreamReader(Request.Body).ReadToEndAsync()).Trim();
                // Determine if a single object or an array was passed in
                if (streamReader.StartsWith("{")) {
                    var node = JsonConvert.DeserializeObject<Address>(streamReader);
                    node.CreatedDate=DateTime.UtcNow;
                    node.CreatedBy=User.Identity.Name ?? "Anonymous";
                    node.LastModifiedDate=DateTime.UtcNow;
                    node.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                    ModelState.Clear();
                    TryValidateModel(node);
                    if (!ModelState.IsValid) {
                        return BadRequest(ModelState);
                    }
                    _db.Addresses.Add(node);
                    await _db.SaveChangesAsync();
                    return Created("", node);
                } else if (streamReader.StartsWith("[")) {
                    var nodes = JsonConvert.DeserializeObject<IEnumerable<Address>>(streamReader);
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
                        _db.Addresses.Add(node);
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
                if (address==null) {
                    return NotFound();
                }
                address.LastModifiedDate=DateTime.UtcNow;
                address.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                addressDelta.Patch(address);
                await _db.SaveChangesAsync();
                return Ok(address);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }

        /// <summary>Bulk edit addresses</summary>
        /// <param name="addressDeltas">An array of partial address objects.  Only properties supplied will be updated.</param>
        /// <returns>An updated list of addresses</returns>
        /// <response code="200">The address was successfully updated</response>
        /// <response code="400">The address is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The address was not found</response>
        [HttpPatch]
        [ODataRoute("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized - User not authenticated
        [ProducesResponseType(typeof(ForbiddenException), 403)] // Forbidden - User does not have required claim roles
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [Authorize]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> PatchBulk([FromBody] IEnumerable<Address> addressDeltas) {
            try {
                Request.Body.Position = 0;
                var patchAddresses = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(await new StreamReader(Request.Body).ReadToEndAsync());
                var addressProperties = typeof(Address).GetProperties();
                // Get list of all passed in Ids
                var idList = new List<int>();
                foreach (var patchAddress in patchAddresses) {
                    idList.Add((int)patchAddress["id"]);
                }
                _db.ChangeTracker.AutoDetectChangesEnabled = false;
                // Make one SQL call to get all the database objects to Patch
                var addresses = await _db.Addresses.Where(st => idList.Contains(st.Id)).ToListAsync();
                // Update each database objects
                foreach (var patchAddress in patchAddresses) {
                    var address = addresses.Find(st => st.Id == (int)patchAddress["id"]);
                    if (address== null) {
                        return NotFound();
                    }
                    // Loop through the changed properties updating the object
                    foreach (var patchAddressProperty in patchAddress.Properties()) {
                        var patchAddressPropertyName = patchAddressProperty.Name;
                        if (patchAddressPropertyName != "id") { // Cannot change the id but it will always be passed in
                            for (int i = 0; i < addressProperties.Length; i++) {
                                if (String.Equals(patchAddressPropertyName, addressProperties[i].Name, StringComparison.OrdinalIgnoreCase)) {
                                    _db.Entry(address).Property(addressProperties[i].Name).CurrentValue = Convert.ChangeType(patchAddressProperty.Value, addressProperties[i].PropertyType);
                                    _db.Entry(address).State = EntityState.Modified;
                                    break;
                                    // Could optionally even support deltas within deltas here
                                }
                            }
                        }
                    }
                    address.LastModifiedDate=DateTime.UtcNow;
                    address.LastModifiedBy=User.Identity.Name ?? "Anonymous";
                    _db.Addresses.Update(address);
                }
                await _db.SaveChangesAsync();
                _db.ChangeTracker.AutoDetectChangesEnabled = true;
                return Ok(addresses);
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

        /// <summary>Associate a user to the address with the given id</summary>
        /// <param name="id">The address id</param>
        /// <param name="reference">The Uri of the user being associated.  Ex: {"@odata.id":"http://api.company.com/odata/users(1)"}</param>
        /// <response code="204">The user was successfully associated</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The user or address was not found</response>
        [HttpPost]
        [ODataRoute("({id})/users/$ref")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [Authorize]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> PostUserRef([FromRoute] int id, [FromBody] ODataReference reference) {
            try {
                Address address = await _db.Addresses.FindAsync(id);
                if (address==null) {
                    return NotFound();
                }
                var userId = Convert.ToInt32(ReferenceHelper.GetKeyFromUrl(reference.uri));
                User user = await _db.Users.FindAsync(userId);
                if (user==null) {
                    return NotFound();
                }
                address.Users.Add(user);
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

        /// <summary>Remove a user association from the address with the given id</summary>
        /// <param name="addressId">The address id</param>
        /// <param name="id">The Uri of the user association being removed.  Ex: id=http://api.company.com/odata/users(1)</param>
        /// <response code="204">The user association was successfully removed</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The user association was not found</response>
        /// <response code="409">SQL Conflict</response>
        [HttpDelete]
        [ODataRoute("({addressId})/users/$ref")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [ProducesResponseType(typeof(string), 409)] // Conflict
        [Authorize]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> DeleteUserRef([FromRoute] int addressId, [FromQuery] Uri id) {
            // This meets the spec, but so does having Uri in [FromBody] so it does not have to use the variable "id" I would prefer to use instead of addressId
            try {
                Address address = await _db.Addresses.Include(e => e.Users).FirstOrDefaultAsync(e => e.Id==addressId);
                if (address==null) {
                    return NotFound();
                }
                var userId = Convert.ToInt32(ReferenceHelper.GetKeyFromUrl(id));
                User user = address.Users.First(e => e.Id==userId);
                if (user==null) {
                    return NotFound();
                }
                address.Users.Remove(user);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message+Constants.messageAppInsights);
            }
        }

        /// <summary>Get the users for the address with the given id</summary>
        /// <param name="id">The address id</param>
        /// <returns>A list of users</returns>
        /// <response code="200">The users were successfully retrieved</response>
        [HttpGet]
        [ODataRoute("({id})/users")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        [Authorize]
        public async Task<IActionResult> GetAddresses([FromRoute] int id) {
            try {
                var users = _db.Addresses.Where(e => e.Id==id).SelectMany(e => e.Users);
                if (!await users.AnyAsync()) {
                    return NotFound();
                }
                return Ok(users);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message+Constants.messageAppInsights);
            }
        }

        /// <summary>Query address notes</summary>
        /// <param name="id">The address id</param>
        /// <returns>A list of notes</returns>
        /// <response code="200">The notes were successfully retrieved</response>
        [HttpGet]
        [ODataRoute("({id})/notes")]
        [ProducesResponseType(typeof(IEnumerable<AddressNote>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        [Authorize]
        public async Task<IActionResult> GetNotes([FromRoute] int id) {
            try {
                var notes = _db.AddressNotes;
                if (!await notes.AnyAsync(n => n.Address.Id == id)) {
                    return NotFound();
                }
                return Ok(notes);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + Constants.messageAppInsights);
            }
        }
 
       #endregion

    }

}
