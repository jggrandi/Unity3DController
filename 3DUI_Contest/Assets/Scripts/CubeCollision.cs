using UnityEngine;
using System.Collections;

public class CubeCollision : MonoBehaviour
{
	//Avoid flickering between green (no collision) and red (collision)
	public float colourChangeDelay = 0.00f;
	float currentDelay = 0f;
	bool colourChangeCollision = false;
	Color collisionColor = new Color(1.0f, 0.0f, 0.0f, 0.9f);
    Color noCollisionColor = new Color(0.0f, 1.0f, 0.0f, 0.6f);
    public float f = 0.0f;

	// Use this for initialization
	void Start ()
	{
	}
	
	void OnCollisionEnter(Collision other) {
		colourChangeCollision = true;
		currentDelay = Time.time + colourChangeDelay;
        f = 1.0f;
	}

	/*void OnCollisionStay(Collision other) {
		colourChangeCollision = true;
		currentDelay = Time.time + colourChangeDelay;
	}*/

	void Update(){
		//if(colourChangeCollision){
        /*if (Time.time > currentDelay)
        {*/
            GameObject.FindGameObjectWithTag("boxSmooth").GetComponent<Renderer>().material.color = collisionColor * f + noCollisionColor * (1.0f - f);
            f *= 0.96f;
        //}
	}

}

