using UnityEngine;
using System.Collections;

public class CubeCollision : MonoBehaviour
{
	public float colourChangeDelay = 0.5f;
	float currentDelay = 0f;
	bool colourChangeCollision = false;
	Color collisionColor = new Color(1.0f, 0.0f, 0.0f, 0.5f); 
	Color noCollisionColor = new Color(0.0f, 1.0f, 0.0f, 0.5f); 

	// Use this for initialization
	void Start ()
	{
	}
	
	void OnCollisionEnter(Collision other) {
		colourChangeCollision = true;
		currentDelay = Time.time + colourChangeDelay;

	}
	
	void Update(){
		if(colourChangeCollision){
			this.GetComponent<Renderer>().material.color = collisionColor;
			if(Time.time > currentDelay){
				this.GetComponent<Renderer>().material.color = noCollisionColor;
				colourChangeCollision = false;
			}
		}
	}

}

