using UnityEngine;
using System.Collections;

public class CameraEffects : MonoBehaviour {
	
	private ParticleSystem lightSpeedEffect;
	
	void Awake()
	{
		lightSpeedEffect = transform.Find("Light Speed Effect").particleSystem;
		lightSpeedEffect.Stop();
	}
	
	void Update()
	{
		lightSpeedEffect.transform.LookAt(Game.mainCamera.transform);
	}
	
	public void ActivateLightSpeed()
	{
		lightSpeedEffect.transform.LookAt(Game.mainCamera.transform.position);
		lightSpeedEffect.Simulate(1);
		lightSpeedEffect.Play();
	}
	
	public void DeactivateLightSpeed()
	{
		lightSpeedEffect.Stop();
	}
	
	//public void SetLightSpeedParticleSpeed();
	
}
