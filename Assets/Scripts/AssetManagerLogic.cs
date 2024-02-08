using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

class AssetManagerLogic : MonoBehaviour
{
	// Towers and Characters
	public enum Characters
	{
		Cow, Vampire, Izual, Builder, Andariel, FireGolem, Wraith, Oger, MaxLength
	};

	public enum CharacterActions
	{
		Idle, Walk, Attack, Dying, Dead, MaxLength
	}
    
	public enum GameObjects
	{
        // Buildings
        StatusBoss, SoulStone,

        // Maps
        Map,

        // UI
        Clicked,
        Target,
        MinimapTarget,
        Line,
        MinimapLine,

        // Characters
        Cow, Vampire, Izual, Builder, Andariel, FireGolem, Wraith, Oger,

        // Particles
        GainGold, Blood, Magic, Fireball, Fireball2, Iceball
    }

	// For singleton ..
	public static AssetManagerLogic instance = null;
	public GameObject[] gameObjects;

	public static T ParseEnum<T>(string value)
	{
		return (T) Enum.Parse(typeof(T), value, true);
	}
    
	public GameObject CreateGameObject(string name, Vector3 pos)
	{
		if (_nameByType.ContainsKey (name)) 
		{
			return Instantiate(gameObjects[_nameByType[name]], pos, Quaternion.identity);
		}
		else 
		{
			Debug.Assert (false, "no prefab! (game object)");
			return null;
		}
	}

	public SortedDictionary<string, SortedDictionary<string, Sprite[]>> SpriteMap
	{
		get { return _spriteMap; }
	}

	public SortedDictionary<string, Sprite> InfoSpriteMap
	{
		get { return _infoSpriteMap; }
	}

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}
		
	// Use this for initialization
	void Start ()
	{
		Debug.Log("AssetManager Start!");
		LoadSpriteMap ();
		LoadGameObjects ();
		LoadInfoSpriteMap ();
	}

	void LoadSpriteMap()
	{
		// Sprite map setting..
		_spriteMap = new SortedDictionary<string, SortedDictionary<string, Sprite[]> > ();

		for (int i = 0; i < (int)Characters.MaxLength; i++)
		{
			string name = ((Characters)i).ToString ();
			_spriteMap [name] = new SortedDictionary<string, Sprite[]> ();

			for (int j = 0; j < (int)CharacterActions.MaxLength; j++) 
			{
				string action = ((CharacterActions)j).ToString ();
				Sprite[] sprites = Resources.LoadAll<Sprite>("page/" + name + "/" + action);

				if (sprites.Length == 0) 
					_spriteMap [name] [action] = null;
				else
					_spriteMap [name] [action] = sprites;
			}
		}
	}

	void LoadInfoSpriteMap()
	{
		_infoSpriteMap = new SortedDictionary<string, Sprite> ();

		Sprite[] sprites = Resources.LoadAll<Sprite>("info/");

		foreach (var s in sprites) 
		{
			_infoSpriteMap [s.name] = s;
		}
	}

	void LoadGameObjects()
	{
		_nameByType = new SortedDictionary<string, int> ();
		char[] delimiter = { ' ' };
        
		for (int i=0; i < gameObjects.Length; i++) 
		{
			string text = gameObjects [i].ToString ();

			string[] words = text.Split(delimiter);
			_nameByType [words[0]] = i;
		}
	}
    
	private SortedDictionary<string, SortedDictionary<string, Sprite[]>> _spriteMap;
	private SortedDictionary<string, Sprite> _infoSpriteMap;
	private SortedDictionary<string, int> _nameByType;
}
	

