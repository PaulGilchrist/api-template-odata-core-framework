using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Classes {
    public class Constants {
        public const string errorSqlDuplicateKey = "duplicate key";
        public const string errorSqlForeignKey = "FOREIGN KEY";
        public const string errorSqlReferenceConflict = "conflicted with the REFERENCE constraint";
        public const string errorSqlDoesNotExist = "does not exist";
        public const string messageDupAssoc = "Association already exists";
        public const string messageDupEntity = "Entity already exists";
        public const string messageerrorSqlForeignKey = "Foreign key conflict found";
        public const string messageAppInsights = "\nSee Application Insights telemetry for full details";
    }
}
