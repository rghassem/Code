using UnityEngine;
using System.Collections;

public class RocketTrail : MonoBehaviour {

	ParticleSystem exhaust;
	ParticleSystem flames;
	
	float defaultFlameEmissionRate;
	bool locked;
	
	public bool isPlaying;
	// Use this for initialization
	void Awake () {
		exhaust = transform.Find("Rocket Smoke Effect").particleSystem;
		flames = transform.Find("Rocket Fire Effect").particleSystem;
		defaultFlameEmissionRate = flames.emissionRate;
		Stop();
		isPlaying = false;
	}
	
	public void Play()
	{
		if(locked)
			return;
		
		flames.Play();
		exhaust.Play();
		isPlaying = true;
	}
	
	public void Stop()
	{
		if(locked)
			return;
		
		flames.Stop();
		exhaust.Stop();
		isPlaying = false;
	}
	
	
	public void PlayBurst(float duration, int emissionMultiplier)
	{
		if(locked)
			return;
		
		flames.emissionRate = flames.emissionRate * emissionMultiplier;
		Play();
		locked = true;
		Invoke("EndBurst", duration);
	}
	
	void EndBurst()
	{
		locked = false;
		Stop();
		flames.emissionRate = defaultFlameEmissionRate;
	}
	
}
