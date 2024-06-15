using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SockExiled.Extension
{
    internal static class ReflectionExtension
    {
        public static bool IsStatic(this PropertyInfo source, bool nonPublic = false) => source.GetAccessors(nonPublic).Any(x => x.IsStatic);
    }
}
