using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Queryable
{
	public Queryable(CellSpacePartition<Entity> cellSpace, Entity owner)
	{
		_owner = owner;
		cellSpace.AddEntity (owner);
	}

	public void HandleRemove(CellSpacePartition<Entity> cellSpace)
	{
		cellSpace.RemoveEntity (_owner);
	}

	public void UpdateCellSpace(CellSpacePartition<Entity> cellSpace, Point2D oldPos)
	{
		cellSpace.UpdateEntity (_owner, oldPos);
	}

	Entity _owner;
}
