using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Models {

    /// <summary>
    /// Represents a user specific note
    /// </summary>
    [Select]
    public class UserNote {
        public UserNote() {}

        /// <summary>
        /// Gets or sets the addressNote identifier
        /// </summary>        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user realted to this note
        /// </summary>
        /// <value>The user.</value>
        [Required]
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        [Required]
        [StringLength(1024)]
        public string Note { get; set; }
    }

    /// <summary>
    /// Represents a user
    /// </summary>
    [Select]
    public class User {
        public User() { }

        /// <summary>
        /// Gets or sets the user identifier
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's first name
        /// </summary>
        /// <value>The user's first name.</value>
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, MinimumLength=2, ErrorMessage="Must be between 2 and 50 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's middle name or initial
        /// </summary>
        /// <value>The user's middle name or initial.</value>
        [Display(Name = "Middle Name")]
        [StringLength(50, MinimumLength=1, ErrorMessage="Must be between 1 and 50 characters")]
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name
        /// </summary>
        /// <value>The user's last name.</value>
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, MinimumLength=2, ErrorMessage="Must be between 2 and 50 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's email address
        /// </summary>
        /// <value>The user's email address.</value>
        [StringLength(150, MinimumLength=3)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's primary phone number
        /// </summary>
        /// <value>The user's primary phone number.</value>
        [StringLength(20, MinimumLength=7)]
        public string Phone { get; set; }

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
        /// Gets a list of addresses for this user
        /// </summary>
        /// <value>The <see cref="IList{T}">list</see> of <see cref="Address">addresses</see>.</value>
        [Contained]
        public List<Address> Addresses { get; set; } = new List<Address>();

        /// <summary>
        /// Gets a list of notes for this user
        /// </summary>
        /// <value>The <see cref="IList{T}">list</see> of <see cref="UserNote">notes</see>.</value>
        [Contained]
        public List<UserNote> Notes { get; set; } = new List<UserNote>();

    }


}
