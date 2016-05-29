using UnityEngine;

public class CheckPoint : MonoBehaviour 
{


    void OnTriggerEnter(Collider other)
    {
		print ("COL");
        // If the object passes through the checkpoint, we activate it
        if (other.tag == "box")
        {
			print ("AQUI");
            
        }
    }
}