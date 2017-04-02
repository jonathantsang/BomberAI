using UnityEngine;
using System.Collections;

public class RobotController : MonoBehaviour {
	//This will be our maximum speed as we will always be multiplying by 1
	public float maxSpeed = 2f;
	//a boolean value to represent whether we are facing left or not
	bool facingLeft = true;
	//Cached components
	private Rigidbody rigidBody;
	private Transform myTransform;
	//a value to represent our Animator
	Animator anim;
	Rigidbody2D rb;
	//Prefabs
	public GameObject bombPrefab; // SET FROM THE GUI!!!

	// Use this for initialization
	void Start () {
		myTransform = transform;
		//set anim to our animator
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();

	}

	void Update(){
		float moveX = Input.GetAxis ("Horizontal");//Gives us of one if we are moving via the arrow keys
		//if we are moving left but not facing left flip, and vice versa
		if (moveX < 0 && !facingLeft) {
			Flip ();
		} else if (moveX > 0 && facingLeft) {
			Flip ();
		}
		float moveY = Input.GetAxis ("Vertical");//Gives us of one if we are moving via the arrow keys
//		Debug.Log(moveY);
		//move our Players rigidbody

		//set our speed
		anim.SetFloat ("Speed",Mathf.Sqrt (Mathf.Abs (moveX) * Mathf.Abs (moveX) + Mathf.Abs (moveY) * Mathf.Abs(moveY)));
		if (Input.GetKeyDown("a"))
		{
			if (rb.position.x >= -4) {
				rb.MovePosition (rb.position += Vector2.left);
			}
		}
		if (Input.GetKeyDown("d"))
		{
			if (rb.position.x <= 4) {
				rb.MovePosition (rb.position += Vector2.right);
			}
		}
		if (Input.GetKeyDown("w"))
		{
			if (rb.position.y <= 4) {
				rb.MovePosition (rb.position += Vector2.up);
			}
		}
		if (Input.GetKeyDown("s"))
		{
			if (rb.position.y >= -4) {
				rb.MovePosition (rb.position += Vector2.down);
			}
		}
		if (Input.GetKeyDown(KeyCode.Space)) { //Drop bomb
			DropBomb();
		}
	}

	// Update is called once per frame
	void FixedUpdate () {

	}

	void DropBomb() {
		Instantiate (bombPrefab, 
			new Vector3 (Mathf.RoundToInt(myTransform.position.x),
				Mathf.RoundToInt(myTransform.position.y),
				Mathf.RoundToInt(myTransform.position.z)),
			bombPrefab.transform.rotation);
	}

	//flip if needed
	void Flip(){
		facingLeft = !facingLeft;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}