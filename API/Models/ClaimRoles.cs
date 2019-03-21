using System.ComponentModel.DataAnnotations;

namespace API.Models {
    public class ClaimRoles {
        [Key]
        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Name { get; set; }
        [Display(Name = "Roles")] //Comma delimited string containing 0 or more roles
        [Required]
        [StringLength(2048)]
        public string Roles { get; set; }
    }
}
