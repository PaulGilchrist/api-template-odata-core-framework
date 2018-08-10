using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ODataCoreTemplate.Models {
    public class User {
        [Key]
        public int Id { get; set; }

        [Display(Name = "First Name")]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(150)] 
        public string Email { get; set; }

        [StringLength(20)]
         public string Phone { get; set; }

        public List<Address> Addresses { get; set; } = new List<Address>();
    }
}
