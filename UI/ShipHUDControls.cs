using UnityEngine;
using System.Collections;

public class ShipHUDControls : MonoBehaviour {
	
	public UISlider fuelBar;
	public UISlider healthBar;
	public GameObject speedDisplay;
	
	private Ship ship;
	
	private TweenPosition fuelBarTween;
	private TweenPosition speedDisplayTween;
	private TweenPosition healthTween;
	
	private UILabel currentSpeedLabel;
	private float refreshRate = 0.1f;
	private float timeTillNextRefresh;

	// Use this for initialization
	void Start () {
		fuelBarTween = fuelBar.GetComponent<TweenPosition>();
		speedDisplayTween = speedDisplay.GetComponent<TweenPosition>();
		healthTween = healthBar.GetComponent<TweenPosition>();
		
		currentSpeedLabel = speedDisplay.transform.Find("SpeedText").Find("Speed").GetComponent<UILabel>();
		
		NGUITools.SetActive(fuelBarTween.gameObject, false);
		NGUITools.SetActive(healthTween.gameObject, false);
	}
	
	void Update()
	{
		if(ship != null)
		{
			Damagable health = ship.GetComponent<Damagable>();
			SetHealthDisplay( health.Health / health.MaxHealth );
			
			if(timeTillNextRefresh <= 0)
			{
				string speed = (ship.GetSpeed() / Game.SPEED_OF_LIGHT).ToString();
				if(speed.Length > 4)
					speed = speed.Substring(0,4);
				currentSpeedLabel.text = speed;
				timeTillNextRefresh = refreshRate;
			}
			else
			{
				timeTillNextRefresh -= Time.deltaTime;
			}
		}
	}
	
	public void Show(Ship activeShip)
	{
		ship = activeShip;
		
		PlayWidgetAnimation(fuelBarTween);	
		PlayWidgetAnimation(speedDisplayTween);
		PlayWidgetAnimation(healthTween);
	}
	
	private void PlayWidgetAnimation(TweenPosition tweener)
	{
		if(!tweener.gameObject.active)
			NGUITools.SetActive(tweener.gameObject, true); //animation will play automatically
		tweener.Play(true);
	}
	
	public void Hide()
	{
		ship = null;
		fuelBarTween.Play(false);
		speedDisplayTween.Play(false);
		healthTween.Play(false);
	}
	
	public void SetFuelDisplay(float fuelPercent)
	{
		fuelBar.sliderValue = fuelPercent;
	}
	
	public void SetHealthDisplay(float healthPercent)
	{
		healthBar.sliderValue = healthPercent;
	}
	
}