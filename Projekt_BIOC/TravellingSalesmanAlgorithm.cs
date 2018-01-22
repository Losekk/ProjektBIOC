using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace Projekt_BIOC
{
    public class TravellingSalesmanAlgorithm
    {
        private readonly Location _startLocation;
        private readonly KeyValuePair<Location[], double>[] _populationWithDistances;

        public TravellingSalesmanAlgorithm(Location startLocation, Location[] destinations, int populationCount)
        {
            _startLocation = startLocation;
            destinations = (Location[])destinations.Clone();

            _populationWithDistances = new KeyValuePair<Location[], double>[populationCount];

            //Populacja bazowa.
            for (int solutionIndex = 0; solutionIndex < populationCount; solutionIndex++)
            {
                var newPossibleDestinations = (Location[])destinations.Clone();


                foreach (Location t in newPossibleDestinations)
                    RandomProvider.FullyRandomizeLocations(newPossibleDestinations);

                var distance = Location.GetTotalDistance(startLocation, newPossibleDestinations);
                var pair = new KeyValuePair<Location[], double>(newPossibleDestinations, distance);

                _populationWithDistances[solutionIndex] = pair;
            }

            Array.Sort(_populationWithDistances, _sortDelegate);
        }


        private static readonly Comparison<KeyValuePair<Location[], double>> _sortDelegate = _Sort;
        private static int _Sort(KeyValuePair<Location[], double> value1, KeyValuePair<Location[], double> value2)
        {
            return value1.Value.CompareTo(value2.Value);
        }

        public IEnumerable<Location> GetBestSolutionSoFar()
        {
            foreach (var location in _populationWithDistances[0].Key)
                yield return location;
        }

        public bool MustDoCrossovers { get; set; }

        public void Reproduce()
        {
            var bestSoFar = _populationWithDistances[0];

            int halfCount = _populationWithDistances.Length / 2;
            for (int i = 0; i < halfCount; i++)
            {
                var parent = _populationWithDistances[i].Key;
                var child1 = _Reproduce(parent);
                var child2 = _Reproduce(parent);

                var pair1 = new KeyValuePair<Location[], double>(child1, Location.GetTotalDistance(_startLocation, child1));
                var pair2 = new KeyValuePair<Location[], double>(child2, Location.GetTotalDistance(_startLocation, child2));
                _populationWithDistances[i * 2] = pair1;
                _populationWithDistances[i * 2 + 1] = pair2;
            }

            // Najlepszy zostaje.
            _populationWithDistances[_populationWithDistances.Length - 1] = bestSoFar;

            Array.Sort(_populationWithDistances, _sortDelegate);
        }

        public void MutateDuplicates()
        {
            bool needToSortAgain = false;
            int countDuplicates = 0;

            var previous = _populationWithDistances[0];
            for (int i = 1; i < _populationWithDistances.Length; i++)
            {
                var current = _populationWithDistances[i];
                if (!previous.Key.SequenceEqual(current.Key))
                {
                    previous = current;
                    continue;
                }

                countDuplicates++;

                needToSortAgain = true;
                RandomProvider.MutateRandomLocations(current.Key);
                _populationWithDistances[i] = new KeyValuePair<Location[], double>(current.Key, Location.GetTotalDistance(_startLocation, current.Key));
            }

            if (needToSortAgain)
                Array.Sort(_populationWithDistances, _sortDelegate);
        }

        private Location[] _Reproduce(Location[] parent)
        {
            var result = (Location[])parent.Clone();

            if (!MustDoCrossovers)
            {

                RandomProvider.MutateRandomLocations(result);
                return result;
            }


            int otherIndex = RandomProvider.GetRandomValue(_populationWithDistances.Length / 2);
            var other = _populationWithDistances[otherIndex].Key;
            RandomProvider._CrossOver(result, other);

            if (RandomProvider.GetRandomValue(10) == 0)
            {
                RandomProvider.MutateRandomLocations(result);
            }

            return result;
        }
    }
}

