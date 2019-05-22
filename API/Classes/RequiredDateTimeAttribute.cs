using System;
using System.ComponentModel.DataAnnotations;

namespace API.Classes {
    public class RequiredDateTimeAttribute : ValidationAttribute {
        public override bool IsValid(object value) {
            DateTime d = Convert.ToDateTime(value);
            return d > DateTime.MinValue;
        }
    }
}
