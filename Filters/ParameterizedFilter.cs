using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFilterPrototyper.Filters
{
    public abstract class ParameterizedFilter : BaseFilter
    {
        public double Parameter { get; set; }


    }
}
