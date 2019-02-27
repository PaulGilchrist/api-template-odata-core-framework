using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using ODataCoreTemplate.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODataCoreTemplate.V1 {
    [ApiVersion("1.0", Deprecated = true)]
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

        /// <summary>Createa new address</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="address">A full address object</param>
        [HttpPost]
        [ODataRoute("")]
        [ProducesResponseType(typeof(User), 201)] // Created
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        //[Authorize]
        public async Task<IActionResult> Post([FromBody] Address address) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();
            return Created("", address);
        }

        /// <summary>Bulk edit addresses</summary>
        /// <remarks>
        /// Does not support updating a property to null.  Use HTTP action PUT if this is a requirement
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="patchAddressList">An object containing an array of partial address objects.  Only properties supplied and not null, will be updated.</param>
        [HttpPatch]
        [ODataRoute("({id})")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] PatchAddressList patchAddressList) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            //var patchAddresses = patchAddressList.value;
            List<Address> dbAddresses = new List<Address>(0);
            System.Reflection.PropertyInfo[] properties = typeof(PatchAddress).GetProperties();
            foreach (PatchAddress patchAddress in patchAddressList.value) {
                var dbAddress = _db.Addresses.Find(patchAddress.Id);
                if (dbAddress == null) {
                    return NotFound();
                }
                // Update any properties that have changed (and are not null)
                foreach (var property in properties) {
                    var propertyValue = patchAddress.GetType().GetProperty(property.Name).GetValue(patchAddress, null);
                    // Only set values that are not null or zero
                    if (propertyValue != null) {
                        dbAddress.GetType().GetProperty(property.Name).SetValue(dbAddress, propertyValue);
                    }
                }
                _db.Entry(dbAddress).State = EntityState.Detached;
                _db.Addresses.Update(dbAddress);
                dbAddresses.Add(dbAddress);
            }
            await _db.SaveChangesAsync();
            return Ok(dbAddresses);
        }

        /// <summary>Replace all data for the address with the given id</summary>
        /// <remarks>
        /// Make sure to secure this action before production release
        /// </remarks>
        /// <param name="id">The address id</param>
        /// <param name="address">A full address object.  Every property will be updated except id.</param>
        [HttpPut]
        [ODataRoute("({id})")]
        [ProducesResponseType(typeof(User), 200)] // Ok
        [ProducesResponseType(typeof(ModelStateDictionary), 400)] // Bad Request
        [ProducesResponseType(typeof(void), 401)] // Unauthorized
        [ProducesResponseType(typeof(void), 404)] // Not Found
        //[Authorize]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Address address) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            Address dbAddress = await _db.Addresses.FindAsync(id);
            if (dbAddress == null) {
                return NotFound();
            }
            _db.Addresses.Update(address);
            await _db.SaveChangesAsync();
            return Ok(address);
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


    }
}
