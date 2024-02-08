using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;

public class World
{
    public enum Relation
    {
        CreateByBuild, CreateByAttack, CreateBySkill
    }


    public int MinSizeX
	{
		get { return _minSizeX; }
	}
	public int MinSizeY
	{
		get { return _minSizeY; }
	}
	public int MaxSizeX
	{
		get { return _maxSizeX; }
	}
	public int MaxSizeY
	{
		get { return _maxSizeY; }
	}

	public CellSpacePartition<Entity> CellSpace
	{
		get { return _cellSpace; }
	}

    public int Step
    {
        get { return _step; }
    }

	public World(
		EngineLogic engine,
		int minSizeX, 
		int minSizeY,
		int maxSizeX, 
		int maxSizeY,
		Point2D pos,
		Point2D heading,
		Point2D side)
	{
		_engine = engine;
		_minSizeX = minSizeX;
		_minSizeY = minSizeY;
		_maxSizeX = maxSizeX;
		_maxSizeY = maxSizeY;
		_pos = pos;
		_heading = heading;
		_side = side;
        _step = 0;

        _entities = new List<Entity> ();
		_createdEntities = new Queue<Entity> ();
		_projectiles = new List<Projectile> ();
		_cellSpace = new CellSpacePartition<Entity> (
			engine,
			maxSizeX - minSizeX,
			maxSizeY - minSizeY,
			15, 15, 1000);

		_boardHolder = new GameObject ("Board").transform;
		InitializeMap();
    }

    // All entity pos comes from local space.
	public Entity CreateCharacter(string name, uint playerId, Point2D pos, Point2D dest, List<KeyValuePair<int, string>> related)
	{
        Point2D noise = new Point2D((int)_engine.RandGen(-5, 5), (int)_engine.RandGen(-5, 5));
        Point2D genPos = pos;
        Entity instance = null;
	
		//query the name for the type
		if (name == "Cow")
			instance = Character.CreateMelee (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos, dest);
		else if (name == "Oger")
			instance = Character.CreateMelee (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos, dest);
		else if (name == "Andariel")
			instance = Character.CreateMelee (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos, dest);
		else if (name == "Vampire")
			instance = Character.CreateRange (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos, dest, "Magic");
		else if (name == "Wraith")
			instance = Character.CreateRange (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos, dest, "Iceball");
		else if (name == "FireGolem")
			instance = Character.CreateRange (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos, dest, "Fireball2");
		else if (name == "Builder") 
		{
            instance = Builder.Create (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos, dest, related);
		}

		_createdEntities.Enqueue (instance);
		return instance;
	}

	public Entity CreateBuilding(string name, uint playerId, Point2D pos)
	{
		Entity instance = null;
        Point2D noise = new Point2D((int)_engine.RandGen(-5, 5), (int)_engine.RandGen(-5, 5));
        Point2D genPos = pos;

        // Query the name for the type
        if (name == "StatusBoss")
			instance = Building.Create (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos);
		
		else if(name == "Izual")
			instance = Tower.Create (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos, "Fireball");
		
		else if(name == "SoulStone")
			instance = Mine.Create (_engine, name, _engine.EntityMgr.IDGen (), playerId, genPos);

		_createdEntities.Enqueue (instance);
		return instance;
	}

	public Projectile CreateProjectile(string name, uint playerId, Point2D pos, uint targetId, int damage)
	{
        Projectile instance = Projectile
			.Create (_engine, name, _engine.EntityMgr.IDGen (), playerId, pos, targetId, damage);
		_projectiles.Add (instance);

		return instance;
	}

    public Entity ClosestEntityFromPos(
        Entity ent, int range, out int distanceSq, Func<Entity, Entity, bool> filter)
    {
        int minDisSq = int.MaxValue;
        Entity minDisEntity = null;
        _cellSpace.CalculateNeighbors(ent.Pos, range);

        for (int i = 0; i < _cellSpace.NeighberCnt; i++)
        {
            Entity e = _cellSpace.Neighbers[i];
            if (filter(ent, e))
                continue;

            int disSq = e.Pos.DistanceSq(ent.Pos);
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                minDisEntity = e;
            }
        }

        distanceSq = minDisSq;
        return minDisEntity;
    }

    public void Update()
	{
        float startTime = Time.realtimeSinceStartup;
        _step++;

        foreach (Entity ent in _entities) 
		{
			ent.Update ();
		}

		while (_createdEntities.Count > 0) 
		{
			_entities.Add (_createdEntities.Dequeue ());
		}

		foreach (Projectile pro in _projectiles) 
		{
			pro.Update ();
		}

		// Remove all dead entities.
		foreach (var e in _entities) 
		{
			if(e.Dead)
				e.HandleRemove();
		}

		_entities.RemoveAll(delegate(Entity e) {
			return e.Dead;
		});

		foreach (var e in _projectiles) 
		{
			if(e.Dead)
				e.HandleRemove();
		}

		_projectiles.RemoveAll(delegate(Projectile e) {
			return e.Dead;
		});

        float collisionStartTime = Time.realtimeSinceStartup;
        
        // Collision
        foreach (var e in _entities)
        {
            IPhysics.EnforceByCellSpace(
                e, _cellSpace, 
                delegate (Entity a, Entity b)
                {
                    return a is Character && b is Character;
                });

            IPhysics.EnforceByCellSpace(
                e, _cellSpace,
                delegate (Entity a, Entity b)
                {
                    return a is Character && b is IBuilding;
                });
        }
    }


	private void InitializeMap()
	{
		Vector3 temp = new Vector3 (_pos.x, _pos.y, 0f);
		GameObject tile = AssetManagerLogic.instance.CreateGameObject("Map", temp);
		tile.transform.SetParent (_boardHolder);
	}

	private EngineLogic _engine;
	private List<Entity> _entities;
	private Queue<Entity> _createdEntities;
	private List<Projectile> _projectiles;
	private CellSpacePartition<Entity> _cellSpace;

    private int _minSizeX, _minSizeY, _maxSizeX, _maxSizeY;
	private Point2D _pos;
	private Point2D _heading;
	private Point2D _side;

	// Just for hold instances.
	private Transform _boardHolder;
    private int _step;
}
