using System;	
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

[SuppressMessage("ReSharper", "IdentifierTypo")]
public class Block : MonoBehaviour
{
    private int blockSize;
    private float startX;
    private float polationX;
    private float freezeY;
    public SpriteRenderer spr;

    private bool freezeX;

    public float limitL;
    public float limitR;
    public bool freezeL;
    public bool freezeR;
    private float freezeXPos;
    
    private static readonly Subject<InitialDrag> StartDrag = new ();
    public static IObservable<InitialDrag> StartDragEvent => StartDrag;
    private static readonly ReactiveProperty<Vector3> OnDrag = new ();
    public static IReadOnlyReactiveProperty<Vector3> OnDragEvent => OnDrag;
    private static readonly Subject<Unit> EndDrag = new ();
    public static IObservable<Unit> EndDragEvent => EndDrag;
    
    private void Start() {
	    blockSize = (int)spr.size.x;
        this.OnCollisionStay2DAsObservable()
            .Where(x => x.gameObject.CompareTag("Wall"))
            .Subscribe(_ => Debug.Log("collision"));
    }

    public Vector3 startPos;
    private void OnMouseDown() {
	    var position = this.transform.position;
        startX = position.x;
        startPos = position;
        //x값에 한해서, block의 pivot값과 마우스 클릭한 지점간의 차이값을 구해서 보정해줘야함.
        freezeY = position.y;
        polationX = position.x - Camera.main!.ScreenToWorldPoint(Input.mousePosition).x;
        SendStartState();
        SendPosition();
    }

    private void OnMouseDrag() {
	    var temp = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
	    var tempBlockX = transform.position.x;
	    
	    if(temp.x + polationX < limitL) {
	        transform.position = new Vector3(limitL, startPos.y, -9);
	        return;
	    }
	    if(limitR < temp.x + polationX) {
		    transform.position = new Vector3(limitR, startPos.y, -9);
		    return;
	    }
        
	    
	    if (freezeL) {
		    if (freezeXPos < temp.x + polationX) {
			    transform.position = temp.GetTransformByFreezeYAndZeroZ(polationX, freezeY);
			    freezeL = false;
			    SendPosition();
			    return;
		    }
	    }
	    if (freezeR) {
		    Debug.Log(temp.x);
		    if (temp.x + polationX < freezeXPos) {
			    transform.position = temp.GetTransformByFreezeYAndZeroZ(polationX, freezeY);
			    freezeR = false;
			    SendPosition();
			    return;
		    }
	    }
        
	    if (limitL <= tempBlockX && tempBlockX <= limitR) {
		    transform.position = temp.GetTransformByFreezeYAndZeroZ(polationX, freezeY);
		    SendPosition();
	    }
	    
	    if (tempBlockX < limitL) {
		    freezeL = true;
		    freezeXPos = transform.position.x;
	    }
	    if (limitR < tempBlockX) {
		    freezeR = true;
		    freezeXPos = transform.position.x;
	    }
    }

    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    private void OnMouseUp() {
        var tempX = this.transform.position.x;
        if(Mathf.Abs(startX - tempX) < 0.5f) {
            transform.position = transform.position.ChangeOnlyX(startX);
        } 
        else {
            if(blockSize % 2 == 1) {
                //even
                transform.position = transform.position.ChangeOnlyX(Mathf.Round(tempX));
            } else { 
                //odd
                /* 
                홀수 길이의 block인 경우,
                L to R : +0.5 
                R to L : -0.5
                */
                if(tempX < startX)  // R to L
                    transform.position = transform.position.ChangeOnlyX(Mathf.Round(tempX+0.5f) - 0.5f);
                else
                    transform.position = transform.position.ChangeOnlyX(Mathf.Round(tempX-0.5f) + 0.5f);
            }
        }

        SendEndState();
    }
    
    public void SetDragLimit((int left, int right) limit) {
	    limitL = startX - limit.left;
	    limitR = startX + limit.right;
	    Debug.Log($"{startX} , {limitL}, {limitR}");
    }
    
    private void SendStartState() {
        Debug.Log("send");
        StartDrag.OnNext(new InitialDrag(this, spr, transform.position, blockSize));
    }
    
    private void SendPosition() {
        OnDrag.Value = this.transform.position;
    }

    private void SendEndState() {
        EndDrag.OnNext(Unit.Default);
    }
}
