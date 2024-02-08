using UnityEngine;
using System.Collections;
using LitJson;
using System;

public class Utils 
{
	public static void TraverseJson(JsonData data)
	{
		if (data.IsArray)
		{
			Debug.Log("[");
			foreach (JsonData j in data)
			{
				TraverseJson(j);
			}
			Debug.Log("]");
		}
		else if (data.IsObject)
		{
			Debug.Log("{");

			foreach (var k in data.Keys)
			{
				Debug.Log(k);
				TraverseJson(data[k]);
			}
			Debug.Log("}");
		}
		else
		{
			Debug.Log(data);
		}
	}

	public static string FindProperty(string json, string propertyKey)
	{
		JsonReader reader = new JsonReader(json);

		while (reader.Read())
		{
			string key = "";

			if (reader.Token == JsonToken.ObjectStart) 
			{
				continue;
			}
			else if (reader.Token == JsonToken.ObjectEnd) 
			{
				Debug.Assert (false, "No propery found! You should check the json string.");
				return "";
			}
			else if (reader.Token == JsonToken.PropertyName)
			{
				key = (string)reader.Value;
			}

			if (key == propertyKey)
				break;
		}

		reader.Read ();
		return (string)reader.Value;
	}

	// 8 Directions
	public enum Directions
	{
		Down, LeftDown, Left, LeftUp, Up, RightUp, Right, RightDown
	}

	public static int MakeDirection(int x, int y, int nu = 12, int de = 5)
	{
		// The line gradient of tan(67.5) == 12(numerator) / 5(denominator).
		int a = nu * y - de * x;
		int b = de * y - nu * x;
		int c = de * y + nu * x;
		int d = nu * y + de * x;

		if (a >= 0 && b <= 0)
			return (int)Directions.RightUp;
		
		else if(b >= 0 && c >= 0)
			return (int)Directions.Up;
		
		else if(c <= 0 && d >= 0)
			return (int)Directions.LeftUp;
		
		else if(d <= 0 && a >= 0)
			return (int)Directions.Left;
		
		else if(a <= 0 && b >= 0)
			return (int)Directions.LeftDown;
		
		else if(b <= 0 && c <= 0)
			return (int)Directions.Down;
		
		else if(c >= 0 && d <= 0)
			return (int)Directions.RightDown;
		
		else
			return (int)Directions.Right;
	}

    public static bool inRect(Point2D bl, Point2D tr, Point2D target)
    {
        return bl.x < target.x && target.x < tr.x &&
            bl.y < target.y && target.y < tr.y; 
    }
}




/*
public void RenderTargetLine(bool show)
{
	if (show) {
		_line.enabled = true;
		Entity ent = _owner.Engine.EntityMgr.GetEntity (_destinationId);
		if (ent == null)
			return;

		ModifyLine (new Vector3 (_owner.Pos.x, _owner.Pos.y, 0), new Vector3 (ent.Pos.x, ent.Pos.y, 0));
	} else {
		_line.enabled = false;
	}
}


public void ModifyLine(Vector3 start, Vector3 end)
	{
		_line.transform.rotation = _lineRotBackUp;
		_line.transform.position = _linePosBackUp;
		_line.transform.localScale = _lineScaleBackUp;

		const int stroke = 2;
		float length = Vector3.Distance(start, end);

		Vector3 preScale = _line.transform.localScale;
		_line.transform.localScale = new Vector3 (preScale.x * length, stroke, preScale.z) ;

		Vector3 source = new Vector3 (1, 0, 0);
		Vector3 target = end - start;

		float angle = Mathf.DeltaAngle(Mathf.Atan2(source.y, source.x) * Mathf.Rad2Deg,
			Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg);

		Vector3 transformVec = new Vector3 (
			0.5f * length * Mathf.Cos(angle * Mathf.Deg2Rad),
			0.5f *  length * Mathf.Sin(angle * Mathf.Deg2Rad), 0);
		
		_line.transform.Rotate (new Vector3 (0, 0, angle));
		_line.transform.position = _line.transform.position + transformVec;
	}*/