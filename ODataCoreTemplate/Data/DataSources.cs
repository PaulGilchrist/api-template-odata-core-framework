using OdataCoreTemplate.Models;
using ODataCoreTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OdataCoreTemplate.Data {
    public static class DataSource {
        private static IList<User> _users { get; set; }

        public static IList<User> GetUsers() {
            if (_users != null) {
                return _users;
            }
            _users = new List<User>();
            // User #1
            User user = new User {
                FirstName = "Paul",
                LastName = "Gilchrist",
                Email = "paul.gilchrist@outlook.com",
                Phone = "123-555-4567"
            };
            _users.Add(user);
            return _users;
        }
    }
}
