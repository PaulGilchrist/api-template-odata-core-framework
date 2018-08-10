using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ODataCoreTemplate.Models {
    public class Address {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StreetNumber { get; set; }

        [Required]
        public string StreetName { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string ZipCode { get; set; }

        public string Name { get; set; }

        public string Suite { get; set; }

        public List<User> Users { get; set; }

    }
}
