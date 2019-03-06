using Microsoft.AspNet.OData;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ODataCoreTemplate.Models {

    public class AddressList {
        public List<Address> value { get; set; }
    }

    public class AddressNote {
        [Key]
        public int Id { get; set; }

        [Required]
        public Address Address { get; set; }

        [Required]
        [StringLength(1024)]
        public string Note { get; set; }
    }

    public class Address {
        public enum AddressType {
            Residential,
            Business
        }
        [Key]
        public int Id { get; set; }

        [Range(0, 99999)]
        [Display(Name = "Street Number")]
        [Required]
        public int StreetNumber { get; set; }

        [Display(Name = "Street Name")]
        [Required]
        [StringLength(100, MinimumLength=2, ErrorMessage="Must be between 2 and 100 characters")]
        public string StreetName { get; set; }

        [Display(Name = "Street Name Line 2")]
        [StringLength(100, MinimumLength=2, ErrorMessage="Must be between 2 and 100 characters")]
        public string StreetName2 { get; set; }

        [Required]
        [StringLength(50, MinimumLength=2, ErrorMessage="Must be between 2 and 50 characters")]
        public string City { get; set; }

        [Required]
        [StringLength(20)]
        public string State { get; set; }

        [Display(Name = "Zip Code")]
        [Required]
        [StringLength(11, MinimumLength=5, ErrorMessage="Must be between 5 and 11 digits")]
        public string ZipCode { get; set; }

        [StringLength(50, MinimumLength=2, ErrorMessage="Must be between 2 and 50 characters")]
        public string Name { get; set; }

        public AddressType? Type { get; set; }

        [StringLength(20, MinimumLength=1, ErrorMessage="Must be between 1 and 20 characters")]
        public string Suite { get; set; }

        public List<AddressNote> Notes { get; set; } = new List<AddressNote>();

        public List<User> Users { get; set; } = new List<User>();
    }


}
