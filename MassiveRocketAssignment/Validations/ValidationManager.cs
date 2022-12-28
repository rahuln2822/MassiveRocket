using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MassiveRocketAssignment.Validation
{
    public static class ValidationManager
    {
        public static T ShouldNotBeNull<T>(this T type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type;
        }
    }
}
