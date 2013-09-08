using UnityEngine;
using System.Collections;

public class Damagable : MonoBehaviour {
	
	public float MaxHealth;
	public float Health;
	public ParticleSystem DestructionEffect;
	public GameObject loot;
	public float DestructionEffectScale = 1;
	
	void Start()
	{
		Health = MaxHealth;
	}
	
	public void TakeHit(Damage damageObject)
	{
		Health -= damageObject.damage;
		if(Health <= 0)
		{
			Die(damageObject.damageType);
		}
	}
	
	void Die(DamageType causeOfDeath)
	{
		ParticleSystem explosionEffect = Instantiate(DestructionEffect) as ParticleSystem;
		explosionEffect.transform.localScale = Vector3.one * DestructionEffectScale;
		explosionEffect.transform.position = transform.position;
		explosionEffect.loop = false;
		explosionEffect.Play();
		Destroy(explosionEffect.gameObject, explosionEffect.duration);
		BroadcastMessage("OnDeath", causeOfDeath, SendMessageOptions.DontRequireReceiver);
		Destroy(gameObject);	
	}
	
	
	void OnCollisionEnter(Collision collisionInfo)
	{
		Damager jerk = collisionInfo.gameObject.GetComponent(typeof(Damager)) as Damager;
		if(jerk != null)
		{
			TakeHit(jerk.GetDamage());
		}
		
	}
	
}
