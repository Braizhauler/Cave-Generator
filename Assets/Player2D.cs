using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2D : MonoBehaviour {

	public float playerSpeed;

	Rigidbody2D rigidBody;
	Vector2 velocity;

	// Use this for initialization
	void Start () {
		rigidBody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		velocity = playerSpeed * new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")).normalized;
	}

	void FixedUpdate()	{
		rigidBody.MovePosition (rigidBody.position + velocity * Time.fixedDeltaTime);
	}
}
