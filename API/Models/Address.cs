using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ODataCoreTemplate.Models {
    public class Address {
        [Key]
        public int Id { get; set; }

        [Range(0, 99999)]
        [Display(Name = "Street Number")]
        [Required]
        public int StreetNumber { get; set; }

        [Display(Name = "Street Name")]
        [Required]
        [StringLength(100)]
        public string StreetName { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(20)]
        public string State { get; set; }

        [Display(Name = "Zip Code")]
        [Required]
        [StringLength(11)]
        public string ZipCode { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Suite { get; set; }

        public List<User> Users { get; set; } = new List<User>();

    }
}
