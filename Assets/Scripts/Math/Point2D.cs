using UnityEngine;
using System.Collections;

public struct Point2D
{
    public int x;
    public int y;

	public Point2D(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Point2D(Point2D p)
	{
		this.x = p.x;
		this.y = p.y;
	}

	public int LengthSq()
	{
		return (x * x) + (y * y);
	}

	public int Length()
	{
		return IMath.Sqrt (LengthSq ());
	}

	public int DistanceSq(Point2D pos)
	{
		return (pos.x - x) * (pos.x - x) + (pos.y - y) * (pos.y - y);
	}

	public static int Dot(Point2D p1, Point2D p2)
	{
		return (p1.x * p2.x) + (p1.y * p2.y);
	}

	public static int Cross(Point2D p1, Point2D p2)
	{
		return (p1.x * p2.x) - (p1.y * p2.y);
	}

	public static bool operator == (Point2D p1, Point2D p2)
	{
		return p1.x == p2.x && p1.y == p2.y;
	}

	public static bool operator != (Point2D p1, Point2D p2)
	{
		return !(p1 == p2);
	}

	public static Point2D operator +(Point2D p1, Point2D p2)
	{
		return new Point2D(p1.x + p2.x, p1.y + p2.y);
	}

	public static Point2D operator -(Point2D p1, Point2D p2)
	{
		return new Point2D(p1.x - p2.x, p1.y - p2.y);
	}

	public static Point2D operator *(Point2D p, int k)
	{
		return new Point2D(p.x * k, p.y * k);
	}

	public static Point2D operator *(int k, Point2D p)
	{
		return new Point2D(p.x * k, p.y * k);
	}

	public static Point2D operator /(Point2D p, int k)
	{
		return new Point2D(p.x / k, p.y / k);
	}
}