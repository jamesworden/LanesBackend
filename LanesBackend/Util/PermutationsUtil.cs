namespace LanesBackend.Util
{
    public class PermutationsUtil
    {
        public static List<List<T>> GetSubsetsPermutations<T>(List<T> list)
        {
            var permutations = new List<List<T>>();
            GenerateSubsetsPermutations(list, 0, permutations);
            return permutations;
        }

        static void GenerateSubsetsPermutations<T>(List<T> list, int startIndex, List<List<T>> result)
        {
            if (startIndex == list.Count)
            {
                return;
            }

            for (int i = startIndex; i < list.Count; i++)
            {
                var subset = list.GetRange(startIndex, i - startIndex + 1);
                permutationsOfSubset(subset, result);
                GenerateSubsetsPermutations(list, i + 1, result);
            }
        }

        static void permutationsOfSubset<T>(List<T> subset, List<List<T>> result)
        {
            var permutations = new List<List<T>>();
            GeneratePermutations(subset, 0, subset.Count - 1, permutations);
            result.AddRange(permutations);
        }

        static void GeneratePermutations<T>(List<T> list, int startIndex, int endIndex, List<List<T>> result)
        {
            if (startIndex == endIndex)
            {
                result.Add(new List<T> { list[startIndex] });
            }
            else
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    Swap(list, startIndex, i);
                    GeneratePermutations(list, startIndex + 1, endIndex, result);
                    Swap(list, startIndex, i); // Backtrack
                }
            }
        }

        static void Swap<T>(List<T> list, int i, int j)
        {
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
