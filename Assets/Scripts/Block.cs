using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    /* 
    block의 이동 범위 : -4 ~ +5
     */
    private int blockLength;
    private Rigidbody2D r2D;
    private RigidbodyConstraints2D defaultCons = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    private float startX;
    private float polationX;
    private float freezeY;
    private SpriteRenderer spr;

    private void Start() {
        r2D = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        blockLength = (int)spr.size.x;
    }

    private void OnMouseDown() {
        startX = this.transform.position.x;
        //x값에 한해서, block의 pivot값과 마우스 클릭한 지점간의 차이값을 구해서 보정해줘야함.
        r2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        freezeY = this.transform.position.y;
        polationX = this.transform.position.x 
            - Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
    }

    private void OnMouseDrag() {
        this.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition)
            .GetTransformByFreezeYAndZeroZ(polationX, freezeY);
    }

    private void OnMouseUp() {
        var tempX = this.transform.position.x;
        if(Mathf.Abs(startX - tempX) < 0.5f) {
            this.transform.position = transform.position.ChangeOnlyX(startX);
        } 
        else {
            if(blockLength % 2 == 1) {
                //even
                this.transform.position = transform.position.ChangeOnlyX(Mathf.Round(tempX));
            } else { 
                //odd
                /* 
                홀수 길이의 block인 경우,
                L to R : +0.5 
                R to L : -0.5
                */
                if(tempX < startX)  // R to L
                    this.transform.position = transform.position.ChangeOnlyX(Mathf.Round(tempX+0.5f) - 0.5f);
                else
                    this.transform.position = transform.position.ChangeOnlyX(Mathf.Round(tempX-0.5f) + 0.5f);
            }
        }
        r2D.constraints = defaultCons;
    }
}
