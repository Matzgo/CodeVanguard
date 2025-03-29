using System.Collections.Generic;
using UnityEngine;

public class SurveyCode : MonoBehaviour
{
    public int Method4(List<int> numbers)
    {
        int value = numbers[0];
        for (int i = 1; i < numbers.Count; i++)
        {
            if (numbers[i] > value)
            {
                value = numbers[i];
            }
        }
        return value;
    }
}
