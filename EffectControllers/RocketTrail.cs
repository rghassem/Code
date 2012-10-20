using UnityEngine;
using System.Collections;

public class RocketTrail : MonoBehaviour {

	ParticleSystem exhaust;
	ParticleSystem flames;
	
	// Use this for initialization
	void Awake () {
		exhaust = transform.Find("Rocket Smoke Effect").particleSystem;
		flames = transform.Find("Rocket Fire Effect").particleSystem;
		//Stop();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void Play()
	{
		flames.Play();
		exhaust.Play();
		
	}
	
	public void Stop()
	{
		flames.Stop();
		exhaust.Stop();
	}
}
