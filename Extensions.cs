using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public static class MyExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var last_index = list.Count - 1;
            while (last_index > 0)
            {
                var randIndex = Random.Shared.Next(0, last_index);
                var tempValue = list[last_index];
                list[last_index] = list[randIndex];
                list[randIndex] = tempValue;
                last_index--;
            }
            return list;
        }
    }
}
