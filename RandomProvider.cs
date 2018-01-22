using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Projekt_BIOC
{
    public static class RandomProvider
    {
        private static readonly Random _random = new Random();

        public static int GetRandomValue(int limit)
        {
            return _random.Next(limit);
        }


        public static void MutateRandomLocations(Location[] locations)
        {

            int mutationCount = GetRandomValue(locations.Length / 10) + 1;
            for (int mutationIndex = 0; mutationIndex < mutationCount; mutationIndex++)
            {
                int index1 = GetRandomValue(locations.Length);
                int index2 = GetRandomValue(locations.Length - 1);
                if (index2 >= index1)
                    index2++;

                switch (GetRandomValue(3))
                {
                    case 0: Location.SwapLocations(locations, index1, index2); break;
                    case 1: Location.MoveLocations(locations, index1, index2); break;
                    case 2: Location.ReverseRange(locations, index1, index2); break;
                    default: throw new InvalidOperationException();
                }
            }
        }

        public static void FullyRandomizeLocations(Location[] locations)
        {

            int count = locations.Length;
            for (int i = count - 1; i > 0; i--)
            {
                int value = GetRandomValue(i + 1);
                if (value != i)
                    Location.SwapLocations(locations, i, value);
            }
        }

        internal static void _CrossOver(Location[] locations1, Location[] locations2)
        {

            var availableLocations = new HashSet<Location>(locations1);

            int startPosition = GetRandomValue(locations1.Length);
            int crossOverCount = GetRandomValue(locations1.Length - startPosition);

            Array.Copy(locations2, startPosition, locations1, startPosition, crossOverCount);
            List<int> toReplaceIndexes = null;

            // usuniecie lokacji uzytych z mozliwych
            int index = 0;
            foreach (var value in locations1)
            {
                if (!availableLocations.Remove(value))
                {
                    if (toReplaceIndexes == null)
                        toReplaceIndexes = new List<int>();

                    toReplaceIndexes.Add(index);
                }

                index++;
            }

            if (toReplaceIndexes != null)
            {

                using (var enumeratorIndex = toReplaceIndexes.GetEnumerator())
                {
                    using (var enumeratorLocation = availableLocations.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumeratorIndex.MoveNext())
                            {
                                Debug.Assert(!enumeratorLocation.MoveNext());
                                break;
                            }

                            if (!enumeratorLocation.MoveNext())
                                throw new InvalidOperationException("Something wrong happened.");

                            locations1[enumeratorIndex.Current] = enumeratorLocation.Current;
                        }
                    }
                }
            }
        }
    }
}

