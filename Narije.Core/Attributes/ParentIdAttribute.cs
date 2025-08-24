using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ParentIdAttribute : Attribute
    {
    }
}
