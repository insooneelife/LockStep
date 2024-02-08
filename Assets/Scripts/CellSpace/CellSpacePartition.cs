using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// 2D integer AABBox
public struct AABBox2D
{
	public int Top { get { return TopRight.y; } }
	public int Left { get { return BotLeft.x; } }
	public int Bottom { get { return BotLeft.y; } }
	public int Right { get { return TopRight.x; } }

	public AABBox2D(Point2D bl, Point2D tr)
	{
        BotLeft = bl;
        TopRight = tr;
	}

	// Returns true if the bbox described by other intersects with this one
	public bool IsOverlappedWith(AABBox2D other)
	{
		return !((other.Top <= this.Bottom) ||
			(other.Bottom >= this.Top) ||
			(other.Left >= this.Right) ||
			(other.Right <= this.Left));
	}
	public bool Collide(Point2D pos)
	{
		return
            BotLeft.x <= pos.x && pos.x <= TopRight.x &&
            BotLeft.y <= pos.y && pos.y <= TopRight.y;
	}

	public void Print()
	{
		Debug.Log(BotLeft.x + ", " + BotLeft.y + "  " + TopRight.x + ", " + TopRight.y);
	}

	public Point2D BotLeft;
    public Point2D TopRight;
};
	

public class Cell<Entity> where Entity : ICellSpaceQueryable
{
    public SortedDictionary<uint, Entity> Members
    {
        get { return _members; }
    }

	public AABBox2D BBox
	{
		get { return _bBox; }
	}
    
    public Cell(EngineLogic engine, Point2D botleft, Point2D topright)    
	{
		_engine = engine;
		_members = new SortedDictionary<uint, Entity>();
		_bBox = new AABBox2D(botleft, topright);

	}

	EngineLogic _engine;

    // All the entities inhabiting this cell
    private SortedDictionary<uint, Entity> _members;


	// The cell's bounding box (it's inverted because the Window's default
	// co-ordinate system has a y axis that increases as it descends)
	private AABBox2D _bBox;
};

public interface ICellSpaceQueryable
{
	Queryable Queryable { get; set; }
	Point2D Pos { get; }
	int Radius { get; }
    bool Ignore { get; }
    uint Id { get; }

    void HandleQueried ();
	void HandleReleaseQuery ();
}

// Defines a cell space containing a vector of cells
public class CellSpacePartition<Entity> where Entity : ICellSpaceQueryable
{
	public Cell<Entity> [] Cells
	{
		get { return _cells; }
	}

    public Entity[] Neighbers
    {
        get { return _neighbers; }
    }

    public int NeighberCnt
    {
        get { return _neighberCnt; }
    }


    public CellSpacePartition(
		EngineLogic engine,
		int width,          // Width of the environment
		int height,         // Height ...
		int cellNumX,       // Number of cells horizontally
		int cellNumY,       // Number of cells vertically
        int maxExpectNum)   // Number of max entity, this space can handle
    {
		_engine = engine;
		_cells = new Cell<Entity>[cellNumX * cellNumY];
        _neighbers = new Entity[maxExpectNum];
        _spaceWidth = (width);
		_spaceHeight = (height);
		_numCellsX = (cellNumX);
		_numCellsY = (cellNumY);

		// Calculate bounds of each cell
		_cellSizeX = width / cellNumX;
		_cellSizeY = height / cellNumY;

        Debug.Log("cell size : " + _cellSizeX + ", " + _cellSizeY);

		// Create the cells
		for (int y = 0; y < _numCellsY; ++y)
		{
			for (int x = 0; x < _numCellsX; ++x)
			{
				int left = x * _cellSizeX;
				int right = left + _cellSizeX;
				int bot = y * _cellSizeY;
				int top = bot + _cellSizeY;

				_cells [y * _numCellsX + x] = 
					new Cell<Entity>(_engine, new Point2D(left, bot), new Point2D(right, top));
			}
		}
	}


	// Show cell_space_line and other data
	public void Render()
	{
		foreach (var c in _cells)
		{
            Point2D bl = ToWorldPos(c.BBox.BotLeft);
            Point2D br = ToWorldPos(new Point2D(c.BBox.Right, c.BBox.Bottom));
            Point2D tr = ToWorldPos(c.BBox.TopRight);
            Point2D tl = ToWorldPos(new Point2D(c.BBox.Left, c.BBox.Top));
            
            Debug.DrawLine(new Vector3(bl.x, bl.y), new Vector3(br.x, br.y), Color.red);
            Debug.DrawLine(new Vector3(br.x, br.y), new Vector3(tr.x, tr.y), Color.red);
            Debug.DrawLine(new Vector3(tr.x, tr.y), new Vector3(tl.x, tl.y), Color.red);
            Debug.DrawLine(new Vector3(tl.x, tl.y), new Vector3(bl.x, bl.y), Color.red);
        }
	}

	// Adds entities to the class by allocating them to the appropriate cell
	public void AddEntity(Entity ent)
	{
		Debug.Assert(ent != null, "Entity is null!");

		int idx = PositionToIndex(ent.Pos);

        if (_cells[idx].Members.ContainsKey(ent.Id))
            _cells[idx].Members[ent.Id] = ent;
        else
            _cells[idx].Members.Add(ent.Id, ent);
    }

	// Removes entitiy from the cell
	public void RemoveEntity(Entity ent)
	{
		Debug.Assert(ent != null, "Entity is null!");

		int idx = PositionToIndex(ent.Pos);
		_cells[idx].Members.Remove(ent.Id);
	}

	// Update an Entity's cell by calling this from your Entity's update method 
	public void UpdateEntity(Entity ent, Point2D oldPos)
	{
        //if the index for the old pos and the new pos are not equal then
        //the Entity has moved to another cell.
        int oldIdx = PositionToIndex(oldPos);
		int newIdx = PositionToIndex(ent.Pos);

		if (newIdx == oldIdx) return;

		// The Entity has moved into another cell so delete from current cell and add to new one
		_cells[oldIdx].Members.Remove(ent.Id);
        if (_cells[newIdx].Members.ContainsKey(ent.Id))
            _cells[newIdx].Members[ent.Id] = ent;
        else
            _cells[newIdx].Members.Add(ent.Id, ent);
    }

    // This method calculates all a target's neighbors and stores them in
    // the neighbor vector. After you have called this method use the begin, 
    // next and end methods to iterate through the vector.
    public void CalculateNeighbors(Point2D wpos, int radius)
	{
		Point2D pos = ToCellSpacePos (wpos);
		AABBox2D boundingBox = new AABBox2D (
				new Point2D (pos.x - radius, pos.y - radius),
				new Point2D (pos.x + radius, pos.y + radius));

        _neighberCnt = 0;
		foreach (var cell in _cells)
		{
			if (cell.BBox.IsOverlappedWith(boundingBox))
			{
				foreach (var e in cell.Members)
				{
                    Entity ent = e.Value;
                    if (ent.Ignore)
                        continue;

					Point2D epos = ToCellSpacePos (ent.Pos);
                    Point2D bl;
                    bl.x = epos.x - ent.Radius;
                    bl.y = epos.y - ent.Radius;

                    Point2D tr;
                    tr.x = epos.x + ent.Radius;
                    tr.y = epos.y + ent.Radius;

                    AABBox2D eBoundingBox;
                    eBoundingBox.BotLeft.x = bl.x;
                    eBoundingBox.BotLeft.y = bl.y;
                    eBoundingBox.TopRight.x = tr.x;
                    eBoundingBox.TopRight.y = tr.y;
                    
					if (boundingBox.IsOverlappedWith (eBoundingBox)) 
					{
                        _neighbers[_neighberCnt++] = ent;
                    }
				}
			}
		}
	}



	// Empties the cells of entities
	public void ClearCells()
	{
		foreach(var c in _cells)
		{
			c.Members.Clear();
		}
	}


	// Given a position in the game space this method determines the relevant cell's index
	private int PositionToIndex(Point2D wpos)
	{
		Point2D pos = ToCellSpacePos (wpos);

		Debug.Assert (0 <= pos.x && pos.x <= _spaceWidth &&
		0 <= pos.y && pos.y <= _spaceHeight, "position out of range!");

        int tempJ = _numCellsX * pos.x;
        int tempI = _numCellsY * pos.y;
        tempJ = tempJ / _spaceWidth;
        tempI = tempI / _spaceHeight;
        tempI = tempI * _numCellsX;

        int idx = tempJ + tempI;

		// If the Entity's position is equal to Point2D(_spaceWidth, _spaceHeight)
		// then the index will overshoot. We need to check for this and adjust
		if (idx > _cells.Length - 1)
			idx = _cells.Length - 1;

		return idx;
	}

	public Point2D ToCellSpacePos(Point2D wPos)
	{
		return new Point2D (wPos.x + (_spaceWidth / 2), wPos.y + (_spaceHeight / 2));
	}

	public Point2D ToWorldPos(Point2D cPos)
	{
		return new Point2D (cPos.x - (_spaceWidth / 2), cPos.y - (_spaceHeight / 2));
	}

	EngineLogic _engine;

	// The required amount of cells in the space
	Cell<Entity> []  _cells;

    Entity[] _neighbers;
    int _neighberCnt;

    // The width and height of the world space the entities inhabit
    int _spaceWidth;
	int _spaceHeight;

	// The number of cells the space is going to be divided up into
	int _numCellsX;
	int _numCellsY;

	int _cellSizeX;
	int _cellSizeY;
};