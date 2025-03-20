using System;
using System.Collections.Generic;
using UnityEngine;

public class EVALCODE : MonoBehaviour
{
    // Start is called before the first frame update
    public void Method3()
    {
        int number = 7;
        if (number % 2 == 0)
        {
            Console.WriteLine("Blue");
        }
        else
        {
            Console.WriteLine("Red");
        }
    }

    public void MET()
    {
        List<string> fruits = new List<string> { "Apple", "Banana", "Cherry" };
        foreach (string fruit in fruits)
        {
            Console.WriteLine(fruit);
        }
    }
}
