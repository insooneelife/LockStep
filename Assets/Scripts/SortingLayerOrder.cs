using UnityEngine;
using System.Collections;

public class SortingLayerOrder : MonoBehaviour {
	public int sortingOrder;
	public string sortingLayerName;

	// Use this for initialization
	void Start () 
	{
		GetComponent<ParticleSystem> ().GetComponent<Renderer> ().sortingLayerName = sortingLayerName;
		GetComponent<ParticleSystem> ().GetComponent<Renderer> ().sortingOrder = sortingOrder;
	}
}
