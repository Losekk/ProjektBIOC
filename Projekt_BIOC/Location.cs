using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt_BIOC
{
    public sealed class Location
    {
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }

        public double GetDistance(Location other)
        {
            int diffX = X - other.X;
            int diffY = Y - other.Y;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }

        public static double GetTotalDistance(Location startLocation, Location[] locations)
        {

            if (locations.Length == 0)
                throw new ArgumentException("Musi być zaznaczony przyanjmniej jeden element");


            double result = startLocation.GetDistance(locations[0]);
            int countLess1 = locations.Length - 1;
            for (int i = 0; i < countLess1; i++)
            {
                var actual = locations[i];
                var next = locations[i + 1];

                var distance = actual.GetDistance(next);
                result += distance;
            }

            result += locations[locations.Length - 1].GetDistance(startLocation);

            return result;
        }

        public static void SwapLocations(Location[] locations, int index1, int index2)
        {

            var location1 = locations[index1];
            var location2 = locations[index2];
            locations[index1] = location2;
            locations[index2] = location1;
        }

        // Zmiana kolejności numerów
        public static void MoveLocations(Location[] locations, int fromIndex, int toIndex)
        {
            var temp = locations[fromIndex];

            if (fromIndex < toIndex)
            {
                for (int i = fromIndex + 1; i <= toIndex; i++)
                    locations[i - 1] = locations[i];
            }
            else
            {
                for (int i = fromIndex; i > toIndex; i--)
                    locations[i] = locations[i - 1];
            }

            locations[toIndex] = temp;
        }

        public static void ReverseRange(Location[] locations, int startIndex, int endIndex)
        {

            if (endIndex < startIndex)
            {
                int temp = endIndex;
                endIndex = startIndex;
                startIndex = temp;
            }

            while (startIndex < endIndex)
            {
                Location temp = locations[endIndex];
                locations[endIndex] = locations[startIndex];
                locations[startIndex] = temp;

                startIndex++;
                endIndex--;
            }
        }
    }
}
