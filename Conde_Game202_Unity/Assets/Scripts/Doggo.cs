using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doggo : MonoBehaviour
{
    public GameObject Marley;               // Reference to the player's position.
    public UnityEngine.AI.NavMeshAgent nav;               // Reference to the nav mesh agent.
    public bool isHotDog = false;
    public GameObject DogMesh;
    public GameObject HotDogMesh;

    void Awake ()
    {

    }

    bool barkonce = false;
    bool barktwice = false;
    void Update ()
    {
        if(Marley.activeSelf == true && isHotDog == false)
        {
        	nav.enabled = true;
            // ... set the destination of the nav mesh agent to the player.
            nav.SetDestination (Marley.transform.position);
            //if doggo is near marley by 1m, do bark
            //if catto does meow in 1m range, turn doggo into hotdoggo (start ScaredIntoHotDog Coroutine)
            //else if doggo collides with cat, eat catto
            float distance = (Marley.transform.position - DogMesh.transform.position).sqrMagnitude;
            if(distance < 50)
            {
            	if(barkonce == false) { DogMesh.GetComponent<AudioSource>().Play(); barkonce = true; }
            	if(Marley.GetComponent<SerialReader>().didMarleyMeow == true) //if marley meows
            	{
            		StartCoroutine(ScaredIntoHotDog(10f));
            	}

            	if(distance < 1.75f)
            	{
            		//eat marley
            		//have marley cry
            		if(barktwice == false) { DogMesh.GetComponent<AudioSource>().Play(); barktwice = true; }
            		if(Marley.GetComponent<SerialReader>().didDoggoEatMarley ==false)
            		{
            		Marley.GetComponent<SerialReader>().DoggoAteMarley();
            		}
            	}
            	else
            	{
            		barktwice = false;
            	}
            	
            }
            else
            {
            	barkonce = false;
            	barktwice = false;
            }
            //Debug.Log(distance.ToString());
            //DogMesh.GetComponent<AudioSource>().Play();
        }
        else if (isHotDog == true)
        {
        	nav.enabled = false;
        }//otherwise
        else
        {
            // ... disable the nav mesh agent.
            nav.enabled = false;
        }
    } 

    IEnumerator ScaredIntoHotDog(float seconds) 
    {
    	isHotDog = true;
    	DogMesh.SetActive(false);
    	HotDogMesh.SetActive(true);
    	//do dog whine
    	HotDogMesh.GetComponent<AudioSource>().Play();

    	yield return new WaitForSeconds(seconds);
    	isHotDog = false;
    	DogMesh.SetActive(true);
    	HotDogMesh.SetActive(false);
    	//do dog bark
    	DogMesh.GetComponent<AudioSource>().Play();
    	
    }
}
