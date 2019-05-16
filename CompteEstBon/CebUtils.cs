#region using

using System.Collections.Generic;
using System.Linq;

#endregion using

namespace CompteEstBon {
    public static class CebUtils {
        public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> self) {
            return self.Select((value, index) => (value, index));
        }
    }
}