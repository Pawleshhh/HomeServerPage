using System;
using System.Collections.Generic;
using System.Text;

namespace HomeServerPage.Shared.Extensions;

public static class NullableExtensions
{
    extension<TNullable>(TNullable? nullable) where TNullable : struct
    {

        public TNullable? Value()
        {
            if (nullable.HasValue)
            {
                return nullable.Value;
            }

            return null;
        }

    }
}
