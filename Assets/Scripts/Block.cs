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
    private RigidbodyConstraints2D defaultCons = 
        RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    private float startX;
    private float polationX;
    private float freezeY;
    public SpriteRenderer spr;
    public bool blocked_L, blocked_R;
    
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

    private void OnMouseDown() {
        startX = this.transform.position.x;
        pastX = startX;
        //x값에 한해서, block의 pivot값과 마우스 클릭한 지점간의 차이값을 구해서 보정해줘야함.
        r2D.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        freezeY = this.transform.position.y;
        polationX = this.transform.position.x 
            - Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        SendStartState();
        SendPosition();
    }

    private float pastX;
    
    private void OnMouseDrag() {
        /*
         * 1. 왼쪽, 오른쪽 이동을 감지해야함. V
         * 2. 현재 한쪽이라도 블록되있는지 감지해야함.
         */
        var temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (temp.x + polationX < startX) { //우측
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

        pastX = transform.position.x;
        SendPosition();
    }

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
        r2D.constraints = defaultCons;
    }

    public bool out_L, out_R;
    public float border_L = -99, border_R = 99;
    public float newStandardX;
    
    public void ChangeBlockedState(GameObject border, bool isLeft, bool value) {
        var width = border.GetComponent<SpriteRenderer>().size.x;
        if (isLeft) {
            blocked_L = value;
            if (value) {
                int borderX = Mathf.RoundToInt(border.GetComponent<Transform>().position.x);
                border_L = width switch {
                    1 => borderX + 1,
                    2 or 3 => borderX + 2,
                    4 or 5 => borderX + 3,
                    _ => border_L
                };
                newStandardX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            }
            else border_L = -99;
        }
        else {
            blocked_R = value;
            if (value) {
                int borderX = Mathf.CeilToInt(border.GetComponent<Transform>().position.x);
                border_R = width switch {
                    1 => borderX - 1,
                    2 or 3 => borderX - 2,
                    4 or 5 => borderX - 3,
                    _ => border_R
                };
                newStandardX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            }
            else border_R = 99;
        }
    }

    private void SendStartState() {
        Debug.Log("send");
        startDrag.OnNext(new InitialDrag(spr, transform.position, blockSize));
    }
    
    private void SendPosition() {
        onDrag.Value = this.transform.position;
    }

    private void SendEndState() {
        endDrag.OnNext(Unit.Default);
    }
}
