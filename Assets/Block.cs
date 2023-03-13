using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Rigidbody2D rigidbody2D;
    private RigidbodyConstraints2D defaultCons = RigidbodyConstraints2D.FreezeRotation;
    private float freezeY;
    private float polationX;


    private void OnMouseDown() {
        //x값에 한해서, block의 pivot값과 마우스 클릭한 지점간의 차이값을 구해서 보정해줘야함.
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        freezeY = this.transform.position.y;
        polationX = this.transform.position.x 
            - Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
    }

    private void OnMouseDrag() {
        this.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition)
            .GetTransformByFreezeYAndZeroZ(polationX, freezeY);
    }

    private void OnMouseUp() {
        rigidbody2D.constraints = defaultCons;
    }
}
