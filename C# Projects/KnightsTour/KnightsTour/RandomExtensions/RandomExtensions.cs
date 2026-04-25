
namespace KnightsTour.RandomExtensions
{
    static class RandomExtensions
    {
        // Shuffles elements in any data type
        public static void Shuffle<T>(this Random rnd, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rnd.Next(n--);
                T temp = array[n];
                array[n] = array[n];
                array[n] = temp;
            }
        }
    }
}
