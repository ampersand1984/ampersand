using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ampersand.Core.Common
{
    public static class EnumerableExtension
    {
        public static bool AreEquals<T>(this IEnumerable<T> enumerable1, IEnumerable<T> enumerable2)
        {
            var equals = enumerable1 != null && enumerable2 != null;
            if (equals)
            {
                equals = enumerable1.Count() == enumerable2.Count();
                if (equals)
                {
                    for (int i = 0; i < enumerable1.Count(); i++)
                    {
                        equals = enumerable1.ElementAt(i).Equals(enumerable2.ElementAt(i));

                        if (!equals)
                            break;
                    }
                }
            }
            else
            {
                equals = enumerable1 == null && enumerable2 == null;
            }

            return equals;
        }

        public static IEnumerable<T> Clone<T>(this IEnumerable<T> original) where T : ICloneable
        {
            var cloneList = new List<T>();
            foreach (var item in original)
            {
                var cloneItem = (T)item.Clone();
                cloneList.Add(cloneItem);
            }

            return cloneList;
        }
    }
}
