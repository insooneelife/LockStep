using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Database
{
	static readonly string[,] unitSet = new string[,]
	{ 
		{"Cow", "Oger", "Andariel", "Vampire", "Wraith", "FireGolem", "Builder"},
		{"Izual", "SoulStone", "StatusBoss", "","","",""}
	};
		
	public class CharacterData
	{
		public string Name;
		public int Radius;
		public int MaxHp;
		public int MoveSpeed;
		public int ViewRange;
		public int AttackRange;
		public int Damage;
		public int AttackFrameDelay;
		public int TrainSpeed;
		public int NeedGold;
		public string Type;
		public string ProjectileName;
		public string buildingName;

		public CharacterData()
		{}

		public CharacterData(CharacterData copy)
		{
			Name = copy.Name;
			Radius = copy.Radius;
			MaxHp = copy.MaxHp;
			MoveSpeed = copy.MoveSpeed;
			ViewRange = copy.ViewRange;
			AttackRange = copy.AttackRange;
			Damage = copy.Damage;
			AttackFrameDelay = copy.AttackFrameDelay;
			TrainSpeed = copy.TrainSpeed;
			NeedGold = copy.NeedGold;
			Type = copy.Type;
			ProjectileName = copy.ProjectileName;
		}
	}

	public class TowerData
	{
		string name = "Izual";
		int radius = 60;
		int maxHp = 1200;
		int viewRange = 450;
		int attackRange = 450;
		int damage = 80;
		int attackFrameDelay = 20;
		int needGold = 200;
		string projectileName = "Fireball";
	}

	public class ProjectileData
	{
		string name;
		int radius = 30;
		int impactRange = 20;
		int moveSpeed = 10;

		string particleName;
	}

	public class Mine
	{
		string name;
		int radius = 50;
		int maxHp = 500;
		int earnGold = 5;

		int needGold;
	}


	public class BuilderData
	{
		string name;
		int radius = 15;
		int maxHp = 500;
		int moveSpeed = 10;

		int needGold;
		int trainSpeed;

		string buildingName;
	}

	public class BuildingData
	{
		public string Name;
		public int Radius;
		public int MaxHp;
        public int NeedGold;

        public BuildingData()
        { }

        public BuildingData(BuildingData copy)
        {
            Name = copy.Name;
            Radius = copy.Radius;
            MaxHp = copy.MaxHp;
            NeedGold = copy.NeedGold;
        }
    }


	public Database(EngineLogic engine)
	{
		_engine = engine;
		CharacterDataSetting ();
        BuildingDataSetting ();
    }

    public BuildingData BuildingDataEntry(string name)
    {
        BuildingData entity = null;
        bool exists = _buildingMap.TryGetValue(name, out entity);
        return entity;
    }

    private void BuildingDataSetting()
    {
        _buildingMap = new SortedDictionary<string, BuildingData>();
        BuildingData data = new BuildingData();

        data.Name = "SoulStone";
        data.Radius = 150;
        data.MaxHp = 500;
        data.NeedGold = 150;
        _buildingMap.Add(data.Name, new BuildingData(data));

        data.Name = "Izual";
        data.Radius = 150;
        data.MaxHp = 500;
        data.NeedGold = 125;
        _buildingMap.Add(data.Name, new BuildingData(data));

        data.Name = "StatusBoss";
        data.Radius = 150;
        data.MaxHp = 500;
        data.NeedGold = 150;
        _buildingMap.Add(data.Name, new BuildingData(data));
    }


    public CharacterData CharacterDataEntry(string name)
	{
		CharacterData entity = null;
		bool exists = _characterMap.TryGetValue (name, out entity);
		return entity;
	}


	private void CharacterDataSetting()
	{
		_characterMap = new SortedDictionary<string, CharacterData> ();

		CharacterData data = new CharacterData ();

		data.Name = "Cow";
		data.Radius = 25;
		data.MaxHp = 500;
		data.MoveSpeed = 10;
		data.ViewRange = 450;
		data.AttackRange = 70;
		data.Damage = 20;
		data.AttackFrameDelay = 10;
		data.TrainSpeed = 75;
		data.NeedGold = 50;
		data.Type = "Melee";
		data.ProjectileName = "";
		_characterMap.Add (data.Name, new CharacterData (data));

		data.Name = "Oger";
		data.Radius = 25;
		data.MaxHp = 1000;
		data.MoveSpeed = 7;
		data.ViewRange = 450;
		data.AttackRange = 70;
		data.Damage = 25;
		data.AttackFrameDelay = 10;
		data.TrainSpeed = 90;
		data.NeedGold = 100;
		data.Type = "Melee";
		data.ProjectileName = "";
		_characterMap.Add (data.Name, new CharacterData (data));

		data.Name = "Andariel";
		data.Radius = 25;
		data.MaxHp = 750;
		data.MoveSpeed = 12;
		data.ViewRange = 450;
		data.AttackRange = 75;
		data.Damage = 35;
		data.AttackFrameDelay = 10;
		data.TrainSpeed = 150;
		data.NeedGold = 125;
		data.Type = "Melee";
		data.ProjectileName = "";
		_characterMap.Add (data.Name, new CharacterData (data));

		data.Name = "Vampire";
		data.Radius = 25;
		data.MaxHp = 500;
		data.MoveSpeed = 8;
		data.ViewRange = 450;
		data.AttackRange = 250;
		data.Damage = 25;
		data.AttackFrameDelay = 10;
		data.TrainSpeed = 100;
		data.NeedGold = 90;
		data.Type = "Range";
		data.ProjectileName = "Magic";
		_characterMap.Add (data.Name, new CharacterData (data));

		data.Name = "Wraith";
		data.Radius = 25;
		data.MaxHp = 400;
		data.MoveSpeed = 10;
		data.ViewRange = 450;
		data.AttackRange = 200;
		data.Damage = 25;
		data.AttackFrameDelay = 10;
		data.TrainSpeed = 100;
		data.NeedGold = 90;
		data.Type = "Range";
		data.ProjectileName = "Iceball";
		_characterMap.Add (data.Name, new CharacterData (data));

		data.Name = "FireGolem";
		data.Radius = 25;
		data.MaxHp = 800;
		data.MoveSpeed = 8;
		data.ViewRange = 450;
		data.AttackRange = 180;
		data.Damage = 27;
		data.AttackFrameDelay = 10;
		data.TrainSpeed = 150;
		data.NeedGold = 120;
		data.Type = "Range";
		data.ProjectileName = "Fireball2";
		_characterMap.Add (data.Name, new CharacterData (data));

		data.Name = "Builder";
		data.Radius = 25;
		data.MaxHp = 800;
		data.MoveSpeed = 8;
		data.ViewRange = 450;
		data.AttackRange = 180;
		data.Damage = 27;
		data.AttackFrameDelay = 10;
		data.TrainSpeed = 150;
		data.NeedGold = 150;
		data.Type = "Builder";
		data.ProjectileName = "";
		data.buildingName = ""; // random for now
		_characterMap.Add (data.Name, new CharacterData (data));
	}

	private EngineLogic _engine;
	private SortedDictionary<string, CharacterData> _characterMap;
    private SortedDictionary<string, BuildingData> _buildingMap;
}


