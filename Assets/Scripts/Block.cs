using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Block : MonoBehaviour
{
    /* 
    block의 이동 범위 : -4 ~ +5
     */
    private int blockSize;
    [SerializeField]private Rigidbody2D r2D;
    private float startX;
    private float polationX;
    private float freezeY;
    public SpriteRenderer spr;

    private bool freezeX;

    public float limit_L, limit_R;
    
    private static Subject<InitialDrag> startDrag = new Subject<InitialDrag>();
    public static IObservable<InitialDrag> StartDragEvent => startDrag;
    private static ReactiveProperty<Vector3> onDrag = new ReactiveProperty<Vector3>();
    public static IReadOnlyReactiveProperty<Vector3> OnDragEvent => onDrag;
    public static Subject<Unit> endDrag = new Subject<Unit>();
    public static IObservable<Unit> EndDragEvent => endDrag;
    
    private void Start() {
        r2D = GetComponent<Rigidbody2D>();
        blockSize = (int)spr.size.x;
        this.OnCollisionStay2DAsObservable()
            .Where(x => x.gameObject.CompareTag("Wall"))
            .Subscribe(_ => Debug.Log("collision"));
    }

    public Vector3 startPos;
    private void OnMouseDown() {
        startX = this.transform.position.x;
        startPos = this.transform.position;
        //x값에 한해서, block의 pivot값과 마우스 클릭한 지점간의 차이값을 구해서 보정해줘야함.
        //r2D.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        freezeY = this.transform.position.y;
        polationX = this.transform.position.x 
            - Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        SendStartState();
        SendPosition();
    }

    private void OnMouseDrag() {
        /*
         * 1. 왼쪽, 오른쪽 이동을 감지해야함. V
         * 2. 현재 한쪽이라도 블록되있는지 감지해야함.
         */
        var temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        //if 좌우 경계를 넘어가면 좌우 경계값으로 이동.하도록!!!!
        if(temp.x + polationX < limit_L) {
	        transform.position = new Vector3(limit_L, startPos.y, -9);
	        return;
        }
        if(limit_R < temp.x + polationX) {
	        transform.position = new Vector3(limit_R, startPos.y, -9);
	        return;
        }
        
        /*if (temp.x + polationX < startX) { //우측
            if (!blocked_L) {
                out_L = false;
                transform.position = temp
                    .GetTransformByFreezeYAndZeroZ(polationX, freezeY);
            }
            else {
                if (newStandardX < temp.x) {
                    blocked_L = false;
                }
            }
        }
        else { //좌측
            if (!blocked_R) {
                out_R = false;
                transform.position = temp
                    .GetTransformByFreezeYAndZeroZ(polationX, freezeY);
            }
            else {
                if (newStandardX > temp.x) {
                    blocked_R = false;
                }
            }
        }

        pastX = transform.position.x;*/
        var tempBlockX = transform.position.x;

        /*if (tempBlockX == limit_L || tempBlockX == limit_R) {
	        
        }
        
        if (temp.x + polationX < pastPos.x) { //시작점 기준 왼쪽으로 이동
	        if(limit_L <= tempBlockX || Math.Abs(tempBlockX - limit_R) < 0.01f){
		        transform.position = temp.GetTransformByFreezeYAndZeroZ(polationX, freezeY);
		        SendPosition();
		        freezeX = false;
	        } else freezeX = true;
	        Debug.Log("A");
        }
        else { //시작점 기준 오른쪽으로 이동
	        Debug.Log("B");
	        if(Math.Abs(tempBlockX - limit_L) < 0.01f || tempBlockX <= limit_R){
		        transform.position = temp.GetTransformByFreezeYAndZeroZ(polationX, freezeY);
		        freezeX = false;
		        SendPosition();
	        }
	        else freezeX = true;
        }*/

        if (freeze_L_X) {
	        Debug.Log("???");
	        if (freezeXPos < temp.x + polationX) {
		        transform.position = temp.GetTransformByFreezeYAndZeroZ(polationX, freezeY);
		        freeze_L_X = false;
		        SendPosition();
		        return;
	        }/*
	        else {
		        //transform.position = new Vector3(freezeXPos, 0, 0).GetTransformByFreezeYAndZeroZ(polationX, freezeY);
		        //transform.position = new Vector3(freezeXPos, startPos.y, -9);
		        transform.position = new Vector3(limit_L, startPos.y, -9);
	        }*/
        }
        if (freeze_R_X) {
	        Debug.Log(temp.x);
	        if (temp.x + polationX < freezeXPos) {
		        transform.position = temp.GetTransformByFreezeYAndZeroZ(polationX, freezeY);
		        freeze_R_X = false;
		        SendPosition();
		        return;
	        }/*
	        else {
		        //transform.position = new Vector3(freezeXPos, 0, 0).GetTransformByFreezeYAndZeroZ(polationX, freezeY);
		        //transform.position = new Vector3(freezeXPos, startPos.y, -9);
		        transform.position = new Vector3(limit_R, startPos.y, -9);
	        }*/
        }
        
        if (limit_L <= tempBlockX && tempBlockX <= limit_R) {
	        transform.position = temp.GetTransformByFreezeYAndZeroZ(polationX, freezeY);
	        SendPosition();
        }
        
        //Debug.Log(tempBlockX);
        //너무 확땡겨서 초과하는 경우만 수정하면 될듯.
        if (tempBlockX < limit_L) {
	        freeze_L_X = true;
	        freezeXPos = transform.position.x;
        }
        if (limit_R < tempBlockX) {
	        freeze_R_X = true;
	        freezeXPos = transform.position.x;
        }
    }

    public bool freeze_L_X;
    public bool freeze_R_X;
    private float freezeXPos;
    private void OnMouseUp() {
        var tempX = this.transform.position.x;
        if(Mathf.Abs(startX - tempX) < 0.5f) {
            this.transform.position = transform.position.ChangeOnlyX(startX);
        } 
        else {
            if(blockSize % 2 == 1) {
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

        SendEndState();
    }

    public bool out_L, out_R;
    public float border_L = -99, border_R = 99;
    public float newStandardX;
    
    public void ChangeBlockedState(GameObject border, bool isLeft, bool value) {
        var width = border.GetComponent<SpriteRenderer>().size.x;
        if (isLeft) {
            //blocked_L = value;
            if (value) {
                int borderX = Mathf.RoundToInt(border.GetComponent<Transform>().position.x);
                border_L = width switch {
                    1 => borderX + 1,
                    2 or 3 => borderX + 2,
                    4 or 5 => borderX + 3,
                    _ => border_L
                };
                //newStandardX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
                newStandardX = border_L;
            }
            else border_L = -99;
        }
        else {
            //blocked_R = value;
            if (value) {
                int borderX = Mathf.CeilToInt(border.GetComponent<Transform>().position.x);
                border_R = width switch {
                    1 => borderX - 1,
                    2 or 3 => borderX - 2,
                    4 or 5 => borderX - 3,
                    _ => border_R
                };
                //newStandardX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
                newStandardX = border_R;
            }
            else border_R = 99;
        }
    }

    public void SetDragLimit((int left, int right) limit) {
	    limit_L = startX - limit.left;
	    limit_R = startX + limit.right;
	    Debug.Log($"{startX} , {limit_L}, {limit_R}");
    }
    
    private void SendStartState() {
        Debug.Log("send");
        startDrag.OnNext(new InitialDrag(this, spr, transform.position, blockSize));
    }
    
    private void SendPosition() {
        onDrag.Value = this.transform.position;
    }

    private void SendEndState() {
        endDrag.OnNext(Unit.Default);
    }
}
