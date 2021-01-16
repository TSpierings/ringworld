using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UseRingGravity : MonoBehaviour
{
  public WorldSettings worldSettings;


  // Update is called once per frame
  void Update()
  {
    var gravityDirection = this.transform.position.normalized;
    gravityDirection.z = 0;
    var body = this.GetComponent<Rigidbody>();

    var playerMovement = new Vector3();
    if (Input.GetKey(KeyCode.W))
    {
      playerMovement = Vector3.Cross(gravityDirection, new Vector3(0, 0, 1));
    }

    if (Input.GetKey(KeyCode.S))
    {
      playerMovement = -Vector3.Cross(gravityDirection, new Vector3(0, 0, 1));
    }

    body.velocity += gravityDirection * 9.81f * Time.deltaTime + playerMovement;

    var bar = Math.Atan2(this.transform.position.y, this.transform.position.x);
    this.transform.rotation = Quaternion.Euler((float)(-bar / Math.PI * 180) - 90, 90, 0);
  }
}
