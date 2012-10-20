using UnityEngine;
using System.Collections;

public class Damager : MonoBehaviour 
{	 
	/// <summary>
	/// The type of damage.
	/// </summary>
	public DamageType damageType;
	
	/// <summary>
	/// The amount of damage, usually deducted from health
	/// </summary>
	public int damage;
	
	/// <summary>
	/// The force to apply, along with damage
	/// </summary>
	public Vector3 momentum;
	
	/// <summary>
	/// The type of force represented by the momentum (eg Impulse).
	/// </summary>
	public ForceMode momentumType;
	
	/// <summary>
	/// Returns damage dealt by this object as a Damage Object.
	/// </summary>
	/// <returns>
	/// The damage.
	/// </returns>
	public Damage GetDamage()
	{
		return new Damage
			(
				this,
				damageType,
				damage,
				momentum,
				momentumType
			);
	}
}


public struct Damage
{
	/// <summary>
	/// Reference to the script controlling the attacking object
	/// </summary>
	public MonoBehaviour attacker;
	 
	/// <summary>
	/// The type of damage.
	/// </summary>
	public DamageType damageType;
	
	/// <summary>
	/// The amount of damage, usually deducted from health
	/// </summary>
	public int damage;
	
	/// <summary>
	/// The force to apply, along with damage
	/// </summary>
	public Vector3 momentum;
	
	/// <summary>
	/// The type of force represented by the momentum (eg Impulse).
	/// </summary>
	public ForceMode momentumType;
	
	public Damage(MonoBehaviour pAttacker, DamageType pDamageType, int pDamage, Vector3 pMomentum, ForceMode pMomentumType)
	{
		attacker = pAttacker;
		damageType = pDamageType;
		damage = pDamage;
		momentum = pMomentum;
		momentumType = pMomentumType;
	}
	
}

public enum DamageType
{
	Impact,
	EnergyWeapon,
	Missile
}
