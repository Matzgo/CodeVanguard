using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class Antenna
{

	public List<int> SortNumbers(List<int> numbers)
	{
List<int> sortedNumbers = new List<int>(numbers); // Clone to avoid modifying the original list
        int n = sortedNumbers.Count;

        for (int i = 0; i < n - 1; i++)
        {
            int minIndex = i; // Assume current index is the smallest

            // Find the minimum element in the remaining list
            for (int j = i + 1; j < n; j++)
            {
                if (sortedNumbers[j] < sortedNumbers[minIndex])
                {
                    minIndex = j; // Update index of the smallest value
                }
            }

            // Swap the found minimum element with the first element of the unsorted part
            int temp = sortedNumbers[i];
            sortedNumbers[i] = sortedNumbers[minIndex];
            sortedNumbers[minIndex] = temp;
        }

        return sortedNumbers;
	}
}