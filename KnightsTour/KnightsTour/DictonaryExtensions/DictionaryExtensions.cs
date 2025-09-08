using KnightsTour.Heuristics;
using KnightsTour.Structs;

namespace KnightsTour.DictonaryExtensions
{
    public static class DictionaryExtensions
    {
        // Method check the contents of a dictonary for an item based on a key
        public static bool Contains_<TKey, TValue>(this Dictionary<TKey, TValue> visitedMoves, TKey item, 
           WarnsdorffRule visitedLocation) 
            where TKey : notnull
            where TValue : IEnumerable<WarnsdorffRule>
        {
            if(visitedMoves != null && visitedMoves.TryGetValue(item, out var locations))
            {
                foreach(var location in locations)
                {
                    if(visitedLocation.boardLocation == location.boardLocation)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Method checks the contents of a Dictonary for a (x,y) coordinate
        public static bool ContainsBackTrackMove<TKey, TValue>(this Dictionary<TKey, TValue> visitedMoves,
          BoardLocation visitedLocation)
           where TKey : notnull
           where TValue : IEnumerable<WarnsdorffRule>
        {
            foreach(var visited in visitedMoves.Values)
            {
                foreach(var isVisited in visited)
                {
                    if (visitedLocation == isVisited.boardLocation)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
