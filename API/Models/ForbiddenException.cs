namespace API.Models {
    public class ForbiddenException {
        public string SecuredColumn { get; set; }
        public string RoleRequired { get; set; }
        public string Description { get; set; }
    }
}
