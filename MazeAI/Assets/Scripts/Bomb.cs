using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour {
	//a holder for our Animator
	Animator anim;
	//a public float for the explosion radius
	public float explodeRadius = 1f;

	// Use this for initialization
	void Start () {
		anim = GetComponent <Animator>();
	}

	// Update is called once per frame
	void Update () {
		//if we are done exploding
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("bombdead")) {
			//destroy all the objects in a radius unless they are tagged Player or hand
			Collider2D[] colliders = Physics2D.OverlapCircleAll (transform.position,explodeRadius);
			foreach(Collider2D col in colliders){
				if (col.tag == "Player"){
					Destroy(col.GetComponent<Collider2D>().gameObject);

				}
			}
			Destroy(this.gameObject);

		}
	}
}