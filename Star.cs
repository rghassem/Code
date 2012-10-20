using UnityEngine;
using System.Collections;

public class Star : SelectableBody {
	
	
	GameObject closeLightObject, distantLightObject;
	LensFlare closeFlare, distantFlare;
	
	private float BRIGHTNESS_MULTIPLIER = 1;
	
	private float radius, maxFlareVisibility, minFlareVisibility, maxDistantFlareVisibility;
	
	// Use this for initialization
	void Start () {
		closeLightObject = transform.Find("Star Light").gameObject;
		closeFlare = closeLightObject.GetComponent<LensFlare>();
		
		distantLightObject = transform.Find("Distant Star Light").gameObject;
		distantFlare = distantLightObject.GetComponent<LensFlare>();
		
		distantFlare.color = gameObject.renderer.material.color;
		
		radius = gameObject.transform.localScale.x / 2 + 0.1f; //0.1 puts the flare definitely outside the body of the star
		maxFlareVisibility = (radius * 2) * 100;
		minFlareVisibility = (radius * 2) * 3;
		
		maxDistantFlareVisibility = (radius * 2) * 250;
	}
	
	// Update is called once per frame
	void Update () {
		
		Vector3 lineToCamera = Game.mainCamera.transform.position - transform.position;
		Vector3 directionToCamera = lineToCamera.normalized;
		float distanceToCamera = lineToCamera.magnitude;
		
		//set the position of the light to face the camera
		closeLightObject.transform.position = transform.position + (directionToCamera * radius);
		distantLightObject.transform.position = transform.position + (directionToCamera * radius);
		
		//set the brightness of the light relative to the distance to the camera
		float baseBrightness = Mathf.Max( 0, 1 - distanceToCamera/maxFlareVisibility );
		closeFlare.brightness = baseBrightness * BRIGHTNESS_MULTIPLIER;
		
		//do the same for distant flare
		baseBrightness = Mathf.Max( 0, 1 - distanceToCamera/maxDistantFlareVisibility );
		distantFlare.brightness = baseBrightness * BRIGHTNESS_MULTIPLIER;
		
		if(distanceToCamera <= minFlareVisibility)
		{
			closeFlare.brightness = 0.0f;
			distantFlare.brightness = 0.0f;
		}
	}
	
	
}
