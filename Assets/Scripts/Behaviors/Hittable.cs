using UnityEngine;
using System.Collections;

public class Hittable
{
	public int Hp
	{
		get { return _hp; }
		set { _hp = value; }
	}

	public int MaxHp 
	{ 
		get{ return _maxHp; }
		set{ _maxHp = value; }
	}

	public Hittable(Entity entity, int maxHp)
	{
		Debug.Assert (entity is IHittable, "entity is not IHittable!");

		_owner = entity;
		_hp = maxHp;
		_maxHp = maxHp;

		_particle = AssetManagerLogic.instance.CreateGameObject(
			"Blood", new Vector3 (_owner.Pos.x, _owner.Pos.y));
		_particle.GetComponent<ParticleSystem>().Pause ();
	}

	public void SetPos(Point2D pos)
	{
		_particle.transform.position = new Vector3 (_owner.Pos.x, _owner.Pos.y);
	}

	public virtual void HasDamaged(int damage)
	{
		if (_owner.Ignore)
			return;

		_particle.GetComponent<ParticleSystem>().Play ();
		Hp = IMath.Max(Hp - damage, 0);

		if (Hp == 0)
		{
			Debug.Assert (_owner is IAnimation, "owner is not IAnimation!");
			SetDying ();
		}
	}

    public virtual void HandleRemove()
    {
        UnityEngine.Object.Destroy(_particle, 3);
    }


    public void SetDying()
	{
		Animation animate = (_owner as IAnimation).Animate;
		animate.SetAction ("Dying");
		_owner.FSM.Fire(Entity.Event.HasToDie);
	}

	private Entity _owner;
	private int _hp;
	private int _maxHp;

	private GameObject _particle;
}