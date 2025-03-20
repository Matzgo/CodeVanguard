using System;
using System.Collections.Generic;

public class ScreenShotCode
{
    public void Method1()
    {
        int total = 0;
        for (int i = 1; i <= 5; i++)
        {
            total += i;
        }
        Console.WriteLine(total);
    }


    public void Method2()
    {
        List<string> fruits = new List<string> { "Apple", "Banana", "Cherry" };
        foreach (string fruit in fruits)
        {
            Console.WriteLine(fruit);
        }
    }

    public void Method3()
    {
        int number = 7;
        if (number % 2 == 0)
        {
            Console.WriteLine("Even");
        }
        else
        {
            Console.WriteLine("Odd");
        }
    }
}
