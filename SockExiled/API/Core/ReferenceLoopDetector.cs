using System.Collections.Generic;

namespace SockExiled.API.Core
{
    internal class ReferenceLoopDetector
    {
        public static bool HasReferenceLoop(object obj)
        {
            return HasReferenceLoop(obj, new HashSet<object>(new ReferenceEqualityComparer()));
        }

        private static bool HasReferenceLoop(object obj, HashSet<object> visited)
        {
            if (obj is null)
                return false;

            // Check if the object has already been visited
            if (visited.Contains(obj))
                return true;

            // Add the current object to the visited set
            visited.Add(obj);

            // Get the type of the object
            var type = obj.GetType();

            // Check all properties of the object
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                    continue;

                var value = property.GetValue(obj);
                if (HasReferenceLoop(value, visited))
                    return true;
            }

            // Remove the current object from the visited set after the check
            visited.Remove(obj);

            return false;
        }

        private class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return obj == null ? 0 : obj.GetHashCode();
            }
        }
    }
}
