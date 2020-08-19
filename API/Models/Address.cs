using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Models {

    [Select]
    public class AddressNote {

        /// <summary>
        /// Represents an address specific note.
        /// </summary>
        public AddressNote() {}

        /// <summary>
        /// Gets or sets the addressNote identifier.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the address realted to this note.
        /// </summary>
        /// <value>The address.</value>
        [Required]
        public Address Address { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        [Required]
        [StringLength(1024)]
        public string Note { get; set; }
    }

    /// <summary>
    /// Represents an address.
    /// </summary>
    [Select]
    public class Address {
        public Address() { }

        public enum AddressType {
            Residential,
            Business
        }

        /// <summary>
        /// Gets or sets the address identifier.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the street number.
        /// </summary>
        /// <value>The street number.</value>
        [Range(0, 99999)]
        [Display(Name = "Street Number")]
        [Required]
        public int StreetNumber { get; set; }

        /// <summary>
        /// Gets or sets the street name.
        /// </summary>
        /// <value>The street name.</value>
        [Display(Name = "Street Name")]
        [Required]
        [StringLength(100, MinimumLength=2, ErrorMessage="Must be between 2 and 100 characters")]
        public string StreetName { get; set; }

        /// <summary>
        /// Gets or sets the optional second line for street name.
        /// </summary>
        /// <value>The street name's second line.</value>
        [Display(Name = "Street Name Line 2")]
        [StringLength(100, MinimumLength=2, ErrorMessage="Must be between 2 and 100 characters")]
        public string StreetName2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        [Required]
        [StringLength(50, MinimumLength=2, ErrorMessage="Must be between 2 and 50 characters")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        [Required]
        [StringLength(20)]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal zip code.
        /// </summary>
        /// <value>The postal zip code.</value>
        [Display(Name = "Zip Code")]
        [Required]
        [StringLength(11, MinimumLength=5, ErrorMessage="Must be between 5 and 11 digits")]
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets address location name.
        /// </summary>
        /// <value>The address location name.</value>
        [StringLength(50, MinimumLength=2, ErrorMessage="Must be between 2 and 50 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of address.
        /// </summary>
        /// <value>The address type.</value>
        public AddressType? Type { get; set; }

        /// <summary>
        /// Gets or sets the suite.
        /// </summary>
        /// <value>The suite.</value>
        [StringLength(20, MinimumLength=1, ErrorMessage="Must be between 1 and 20 characters")]
        public string Suite { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the address was first created
        /// </summary>
        /// <value>The address's created date (in UTC)</value>
        public System.DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the name of who created the address
        /// </summary>
        /// <value>The address's createdBy name (in UTC)</value>
        [StringLength(50, MinimumLength = 1, ErrorMessage = "CreatedBy must be between 1 and 50 characters")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the address was last modified
        /// </summary>
        /// <value>The address's last modified date (in UTC)</value>
        public System.DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the name of who last modified the address
        /// </summary>
        /// <value>The address's lastModifiedBy name (in UTC)</value>
        [StringLength(50, MinimumLength = 1, ErrorMessage = "LastModifiedBy must be between 1 and 50 characters")]
        public string LastModifiedBy { get; set; }

        /// <summary>
        /// Gets a list of notes for this address.
        /// </summary>
        /// <value>The <see cref="IList{T}">list</see> of <see cref="AddressNote">notes</see>.</value>
        [Contained]
        public List<AddressNote> Notes { get; set; } = new List<AddressNote>();

        /// <summary>
        /// Gets a list of users for this address.
        /// </summary>
        /// <value>The <see cref="IList{T}">list</see> of <see cref="User">users</see>.</value>
        [Contained]
        public List<User> Users { get; set; } = new List<User>();

    }


}
