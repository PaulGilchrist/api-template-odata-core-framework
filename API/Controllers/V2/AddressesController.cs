using API.Classes;
using API.Configuration;
using API.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
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

namespace API.Controllers.V2 {
    [ApiVersion("2.0")]
    [ODataRoutePrefix("addresses")]
    public class AddressesController : ODataController {
        private ApiDbContext _db;
        private TelemetryTracker _telemetryTracker;

        public AddressesController(ApiDbContext context, TelemetryTracker telemetryTracker) {
            _db = context;
            _telemetryTracker=telemetryTracker;
        }

        #region CRUD Operations

        /// <summary>Query addresses</summary>
        [HttpGet]
        [ODataRoute("")]
        [ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        public async Task<IActionResult> Get() {
            try {
                var addresses = _db.Addresses;
                if (!await addresses.AnyAsync()) {
                    return NotFound();
                }
                return Ok(addresses);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + "\nSee Application Insights Telemetry for full details");
            }
        }

        /// <summary>Query addresses by id</summary>
        /// <param name="id">The address id</param>
        [HttpGet]
        [ODataRoute("({id})")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select | AllowedQueryOptions.Expand)]
        public async Task<IActionResult> GetSingle([FromRoute] int id) {
            try {
                var address = _db.Addresses.Where(e => e.Id==id);
                if (!await address.AnyAsync()) {
                    return NotFound();
                }
                return Ok(address);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + "\nSee Application Insights Telemetry for full details");
            }
        }

        /// <summary>Create one or more new address</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="addressList">An object containing an array of full address objects</param>
        [HttpPost]
        [ODataRoute("")]
        [ProducesResponseType(typeof(User), 201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[Authorize]
        public async Task<IActionResult> Post([FromBody] AddressList addressList) {
            try {
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }
                var addresses = addressList.value;
                foreach (Address address in addresses) {
                    // If anything else uniquely identifies a user, check for it here before allowing POST therby supporting idempotent POST (409 Conflict)
                    address.CreatedDate=DateTime.UtcNow;
                    address.CreatedBy=User.Identity.Name;
                    address.LastModifiedDate=DateTime.UtcNow;
                    address.LastModifiedBy=User.Identity.Name;
                    _db.Addresses.Add(address);
                }
                await _db.SaveChangesAsync();
                return Created("", addresses);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + "\nSee Application Insights Telemetry for full details");
            }
        }

        /// <summary>Bulk edit addresses</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="addressList">An object containing an array of partial address objects.
        /// </param>
        [HttpPatch]
        [ODataRoute("")]
        [ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> Patch([FromBody] AddressList addressList) {
            // Swagger will document a UserList object model, but what is actually being passed in is a dynamic list since PATCH does not require the full object properties
            //     This mean we actually need a DynamicList, so reposition and re-read the body
            //     Full explaination ... https://github.com/PaulGilchrist/documents/blob/master/articles/api/api-odata-bulk-updates.md
            try {
                Request.Body.Position = 0;
                var patchAddressList = JsonConvert.DeserializeObject<DynamicList>(new StreamReader(Request.Body).ReadToEnd());
                var patchAddresses = patchAddressList.value;
                List<Address> dbAddresses = new List<Address>(0);
                System.Reflection.PropertyInfo[] addressProperties = typeof(Address).GetProperties();
                foreach (JObject patchAddress in patchAddresses) {
                    var address = _db.Addresses.Find((int)patchAddress["id"]);
                    if (address== null) {
                        return NotFound();
                    }
                    var patchAddressProperties = patchAddress.Properties();
                    // Loop through the changed properties updating the address object
                    foreach (var patchAddressProperty in patchAddressProperties) {
                        foreach (var addressProperty in addressProperties) {
                            if (String.Compare(patchAddressProperty.Name, addressProperty.Name, true) == 0) {
                                _db.Entry(address).Property(addressProperty.Name).CurrentValue = Convert.ChangeType(patchAddressProperty.Value, addressProperty.PropertyType);
                                // Could optionally even support deltas within deltas here
                            }
                        }
                    }
                    address.LastModifiedDate=DateTime.UtcNow;
                    address.LastModifiedBy=User.Identity.Name;
                    _db.Entry(address).State = EntityState.Detached;
                    _db.Addresses.Update(address);
                    dbAddresses.Add(address);
                }
                await _db.SaveChangesAsync();
                return Ok(dbAddresses);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + "\nSee Application Insights Telemetry for full details");
            }
        }

        /// <summary>Replace all data for an array of addresses</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="addressList">An object containing an array of full address objects.  Every property will be updated except id.</param>
        [HttpPut]
        [ODataRoute("")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> Put([FromBody] AddressList addressList) {
            try {
                var addresses = addressList.value;
                foreach (Address address in addresses) {
                    if (!ModelState.IsValid) {
                        return BadRequest(ModelState);
                    }
                    Address dbAddress = await _db.Addresses.FindAsync(address.Id);
                    if (dbAddress == null) {
                        return NotFound();
                    }
                    address.LastModifiedDate=DateTime.UtcNow;
                    address.LastModifiedBy=User.Identity.Name;
                    _db.Addresses.Update(address);
                }
                await _db.SaveChangesAsync();
                return Ok(addresses);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + "\nSee Application Insights Telemetry for full details");
            }
        }

        /// <summary>Delete the given address</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The address id</param>
        [HttpDelete]
        [ODataRoute("({id})")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
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
                if (ex.InnerException.InnerException.Message.Contains(Constants.deleteForeignKey)) {
                    // Reset the remove    
                    foreach (EntityEntry entityEntry in _db.ChangeTracker.Entries().Where(e => e.State==EntityState.Deleted)) {
                        entityEntry.State=EntityState.Unchanged;
                    }
                    _telemetryTracker.TrackException(ex);
                    return StatusCode(409, "Conflict - Database foreign key constraints prevent this object from being deleted");
                } else {
                    _telemetryTracker.TrackException(ex);
                    return StatusCode(500, ex.Message+"\nSee Application Insights Telemetry for full details");
                }
            }
        }

        #endregion

        #region REFs

        /// <summary>Associate a user to the address with the given id</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The address id</param>
        /// <param name="reference">The Uri of the user being associated.  Ex: {"@odata.id":"http://api.company.com/odata/users(1)"}</param>
        [HttpPost]
        [ODataRoute("({id})/users/$ref")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostUserRef([FromRoute] int id, [FromBody] ODataReference reference) {
            try {
                Address address = await _db.Addresses.FindAsync(id);
                if (address==null) {
                    return NotFound();
                }
                var userId = Convert.ToInt32(ReferenceHelper.GetKeyFromUrl(reference.uri));
                if (address.Users.Any(i => i.Id==userId)) {
                    return StatusCode(409, string.Format("Conflict - The address with id {0} is already linked to the user with id {1}", id, userId));
                }
                User user = await _db.Users.FindAsync(userId);
                if (user==null) {
                    return NotFound();
                }
                address.Users.Add(user);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message+"\nSee Application Insights Telemetry for full details");
            }
        }

        /// <summary>Remove a user association from the address with the given id</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="addressId">The address id</param>
        /// <param name="id">The Uri of the user association being removed.  Ex: id=http://api.company.com/odata/users(1)</param>
        [HttpDelete]
        [ODataRoute("({addressId})/users/$ref")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize(Roles = "Admin")]
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
                return StatusCode(500, ex.Message+"\nSee Application Insights Telemetry for full details");
            }
        }

        /// <summary>Get the users for the address with the given id</summary>
        /// <param name="id">The address id</param>
        [HttpGet]
        [ODataRoute("({id})/users")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        public async Task<IActionResult> GetAddresses([FromRoute] int id) {
            try {
                var users = _db.Addresses.Where(e => e.Id==id).SelectMany(e => e.Users);
                if (!await users.AnyAsync()) {
                    return NotFound();
                }
                return Ok(users);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message+"\nSee Application Insights Telemetry for full details");
            }
        }

        /// <summary>Query address notes</summary>
        [HttpGet]
        [ODataRoute("({id})/notes")]
        [ProducesResponseType(typeof(IEnumerable<AddressNote>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        public async Task<IActionResult> GetNotes([FromRoute] int id) {
            try {
                var notes = _db.AddressNotes;
                if (!await notes.AnyAsync(n => n.Address.Id == id)) {
                    return NotFound();
                }
                return Ok(notes);
            } catch (Exception ex) {
                _telemetryTracker.TrackException(ex);
                return StatusCode(500, ex.Message + "\nSee Application Insights Telemetry for full details");
            }
        }
 
       #endregion

    }

}
