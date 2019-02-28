using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ODataCoreTemplate.Models {

    public class UserList {
        public List<User> value { get; set; }
    }

    public class User {
        [Key]
        public int Id { get; set; }
        [Display(Name = "First Name")]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Display(Name = "Middle Name")]
        [StringLength(50)]
        public string MiddleName { get; set; }
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
