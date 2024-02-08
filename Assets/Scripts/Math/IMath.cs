using UnityEngine;
using System.Collections;

public class IMath
{
	public static int Square(int a)
	{
		return a * a;
	}

	public static int Dis2(int x1, int y1, int x2, int y2)
	{ 
		return Square(x2 - x1) + Square(y2 - y1);
	}

	public static int Dis(int x1, int y1, int x2, int y2)
	{
		return Sqrt(Square(x2 - x1) + Square(y2 - y1));
	}

    // Finds the integer square root of a positive number  
    public static int Sqrt(int num)
    {
        // Avoid zero divide
        if (0 == num) { return 0; }
        if (1 == num) { return 1; }

        // Initial estimate, never low  
        int temp = (num / 2);
        int n = temp + 1;
        int n1 = (n + temp) / 2;
        while (n1 < n)
        {
            n = n1;
            int tempIn = (num / n);
            n1 = (n + tempIn) / 2;
        }
        return n;
    }

    // Returns true if p1 and p2 are in range distance.
    public static bool InRange(Point2D p1, Point2D p2, int range)
	{
		return Dis2(p1.x, p1.y, p2.x, p2.y) <= Square(range);
	}

	public static uint Max(uint a, uint b)
	{
		if (a > b)
			return a;
		else
			return b;
	}

	public static int Max(int a, int b)
	{
		if (a > b)
			return a;
		else
			return b;
	}

	public static uint Min(uint a, uint b)
	{
		if (a < b)
			return a;
		else
			return b;
	}

	public static int Min(int a, int b)
	{
		if (a < b)
			return a;
		else
			return b;
	}
}