using UnityEngine;
using System.Collections;

public class CubeCollision : MonoBehaviour
{
	Color collisionColor = new Color(1.0f, 0.0f, 0.0f, 0.9f);
    Color noCollisionColor = new Color(0.0f, 1.0f, 0.0f, 0.6f);
    public float f = 0.0f;

	// Use this for initialization
	void Start ()
	{
	}
	
	void OnCollisionEnter(Collision other) {
        f = 1.0f;
	}

	/*void OnCollisionStay(Collision other) {
		colourChangeCollision = true;
		currentDelay = Time.time + colourChangeDelay;
	}*/

	void Update(){
            GameObject.FindGameObjectWithTag("boxSmooth").GetComponent<Renderer>().material.color = collisionColor * f + noCollisionColor * (1.0f - f);
            f *= 0.96f;
	}

}

