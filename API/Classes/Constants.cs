using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Classes {
    public class Constants {
        public const string dupKey = "duplicate key";
        public const string foreignKey = "FOREIGN KEY";
        public const string deleteForeignKey = "conflicted with the REFERENCE constraint";
        public const string errorDupAssoc = "Association already exists";
        public const string errorDupEntity = "Entity already exists";
        public const string errorForeignKey = "Foreign key conflict found";
        public const string seeAppInsights = Constants.seeAppInsights;
    }
}
