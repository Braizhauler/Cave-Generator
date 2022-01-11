using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float playerSpeed;

	Rigidbody rigidBody;
	Vector3 velocity;

	// Use this for initialization
	void Start () {
		rigidBody = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		velocity = playerSpeed * new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;
	}

	void FixedUpdate()	{
		rigidBody.MovePosition (rigidBody.position + velocity * Time.fixedDeltaTime);
	}
}
