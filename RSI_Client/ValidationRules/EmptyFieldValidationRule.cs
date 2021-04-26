using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RSI_Client.ValidationRules
{
    public class EmptyFieldValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value==null)
            {
                return new ValidationResult(false, "Field cannot be empty");
            }
            else
            {
                if(value.ToString().Length<4)
                {
                    return new ValidationResult(false, "Cannot be less than 4 characters");
                }
                return ValidationResult.ValidResult;
            }
        }
    }
}
