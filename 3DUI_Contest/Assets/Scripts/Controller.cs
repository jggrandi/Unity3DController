using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	public float speed;
	public GameObject lalal;
	Quaternion origRotation;
	Vector3 origScale;

	// Use this for initialization
	void Start () {
		origRotation = transform.rotation;
		origScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.A)) {
			transform.Translate(Vector3.left * Time.deltaTime * speed);
		}

		if (Input.GetKey (KeyCode.D)) {
			transform.Translate(Vector3.right * Time.deltaTime * speed);
		} 
		
		if (Input.GetKey (KeyCode.S)) {
			transform.Translate(Vector3.back * Time.deltaTime * speed);
		} 
		
		if (Input.GetKey (KeyCode.W)) {
			transform.Translate(Vector3.forward * Time.deltaTime * speed);
		} 

		if (Input.GetKey (KeyCode.Z)) {
			transform.Translate(Vector3.up * Time.deltaTime * speed);
		} 
		
		if (Input.GetKey (KeyCode.X)) {
			transform.Translate(Vector3.down * Time.deltaTime * speed);
		}

		if (Input.GetMouseButton (0)) {
			if (Input.GetAxis ("Mouse Y") < 0) {
				this.transform.Rotate (Vector3.left * 1f);
			}
			if (Input.GetAxis ("Mouse Y") > 0) {
				this.transform.Rotate (Vector3.right * 1f);
			}

			if (Input.GetAxis ("Mouse X") < 0) {
				this.transform.Rotate (Vector3.up * 1f);
			}
			if (Input.GetAxis ("Mouse X") > 0) {
				this.transform.Rotate (Vector3.down * 1f);
			}

			if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
				transform.localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
			}
			if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
				transform.localScale += new Vector3 (0.1f, 0.1f, 0.1f);
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			transform.rotation = origRotation;
			transform.localScale = origScale;
		}
	}
}
