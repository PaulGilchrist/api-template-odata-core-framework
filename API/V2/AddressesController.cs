using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using ODataCoreTemplate.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataCoreTemplate.V2 {
    [ApiVersion("2.0")]
    [ODataRoutePrefix("addresses")]
    public class AddressesController : ODataController {
        private OdataCoreTemplate.Models.ApiDbContext _db;

        public AddressesController(OdataCoreTemplate.Models.ApiDbContext context) {
            _db = context;
        }

        /// <summary>Query addresses</summary>
        [HttpGet]
        [ODataRoute("")]
        [ProducesResponseType(typeof(IEnumerable<Address>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        public async Task<IActionResult> Get() {
            var addresses = _db.Addresses;
            if (!await addresses.AnyAsync()) {
                return NotFound();
            }
            return Ok(addresses);
        }

        /// <summary>Query addresses by id</summary>
        /// <param name="id">The address id</param>
        [HttpGet]
        [ODataRoute("({id})")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)] // Not Found
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select | AllowedQueryOptions.Expand)]
        public async Task<IActionResult> GetSingle([FromRoute] int id) {
            Address address = await _db.Addresses.FindAsync(id);
            if (address == null) {
                return NotFound();
            }
            return Ok(address);
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
            var addresses = addressList.value;
            foreach (Address address in addresses) {
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }
                _db.Addresses.Add(address);
            }
            await _db.SaveChangesAsync();
            return Created("", addresses);
        }

        ///// <summary>Bulk edit addresses</summary>
        ///// <remarks>
        ///// Make sure to secure this action before production release
        ///// </remarks>
        ///// <param name="deltaAddressList">An object containing an array of partial address objects.  Only properties supplied will be updated.</param>
        //[HttpPatch]
        //[ODataRoute("")]
        //[ProducesResponseType(typeof(User), 200)] // Ok
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        //[ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[ProducesResponseType(typeof(void), 404)] // Not Found
        ////[Authorize]
        //public async Task<IActionResult> Patch([FromBody] DeltaAddressList deltaAddressList) {
        //    var deltaAddresses = deltaAddressList.value;
        //    Address[] dbAddresses = new Address[0];
        //    foreach (Delta<Address> deltaAddress in deltaAddresses) {
        //        if (!ModelState.IsValid) {
        //            return BadRequest(ModelState);
        //        }
        //        var dbAddress = _db.Addresses.Find(deltaAddress.GetInstance().Id);
        //        if (dbAddress == null) {
        //            return NotFound();
        //        }
        //        deltaAddress.Patch(dbAddress);
        //        dbAddresses.Append(dbAddress);
        //    }
        //    await _db.SaveChangesAsync();
        //    return Ok(dbAddresses);
        //}

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
            var addresses = addressList.value;
            foreach (Address address in addresses) {
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }
                Address dbAddress = await _db.Addresses.FindAsync(address.Id);
                if (dbAddress == null) {
                    return NotFound();
                }
                _db.Addresses.Update(address);
            }
            await _db.SaveChangesAsync();
            return Ok(addresses);
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
            Address address = await _db.Addresses.FindAsync(id);
            if (address == null) {
                return NotFound();
            }
            _db.Addresses.Remove(address);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Get the users for the address with the given id</summary>
        /// <param name="id">The address id</param>
        [HttpGet]
        [ODataRoute("({id})/users")]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)] // Ok
        [ProducesResponseType(typeof(void), 404)]  // Not Found
        [EnableQuery]
        public IQueryable<User> GetUsers([FromRoute] int id) {
            return _db.Addresses.Where(m => m.Id == id).SelectMany(m => m.Users);
        }


        /// <summary>Associate a user to the address with the given id</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The user id</param>
        /// <param name="userId">The user id to associate with the address</param>
        [HttpPost]
        [ODataRoute("({id})/users({userId})")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> LinkAddresses([FromRoute] int id, [FromRoute] int userId) {
            Address address = await _db.Addresses.FindAsync(id);
            if (address == null) {
                return NotFound();
            }
            if (address.Users.Any(i => i.Id == userId)) {
                return BadRequest(string.Format("The address with id {0} is already linked to the user with id {1}", id, userId));
            }
            User user = await _db.Users.FindAsync(userId);
            if (user == null) {
                return NotFound();
            }
            address.Users.Add(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }


        /// <summary>Remove an user association from the address with the given id</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The address id</param>
        /// <param name="userId">The user id to remove association from the address</param>
        [HttpDelete]
        [ODataRoute("({id})/users({userId})")]
        [ProducesResponseType(typeof(void), 204)] // No Content
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        // [Authorize]
        public async Task<IActionResult> UnlinkAddresses([FromRoute] int id, [FromRoute] int userId) {
            Address address = await _db.Addresses.FindAsync(id);
            if (address == null) {
                return NotFound();
            }
            User user = await _db.Users.FindAsync(userId);
            if (user == null) {
                return NotFound();
            }
            address.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
