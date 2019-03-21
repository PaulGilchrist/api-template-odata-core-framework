using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Models {

    public class UserList {
        public List<User> value { get; set; }
    }

    public class UserNote {
        [Key]
        public int Id { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        [StringLength(1024)]
        public string Note { get; set; }
    }

    public class User {
        [Key]
        public int Id { get; set; }

        [Display(Name = "First Name")]
        [Required]
        [StringLength(50, MinimumLength=2, ErrorMessage="Must be between 2 and 50 characters")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(50, MinimumLength=1, ErrorMessage="Must be between 1 and 50 characters")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(50, MinimumLength=2, ErrorMessage="Must be between 2 and 50 characters")]
        public string LastName { get; set; }

        [StringLength(150, MinimumLength=3)]
        public string Email { get; set; }

        [StringLength(20, MinimumLength=7)]
        public string Phone { get; set; }

        public List<Address> Addresses { get; set; } = new List<Address>();

        public List<UserNote> Notes { get; set; } = new List<UserNote>();

        [Required]
        public System.DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "CreatedBy must be between 1 and 50 characters")]
        public string CreatedBy { get; set; }

        [Required]
        public System.DateTime LastModifiedDate { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "LastModifiedBy must be between 1 and 50 characters")]
        public string LastModifiedBy { get; set; }
    }


}
