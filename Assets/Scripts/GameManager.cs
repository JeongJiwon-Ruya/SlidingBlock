using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class InitialDrag {
    public SpriteRenderer spriteRenderer;
    public Vector3 pos;

    public InitialDrag(SpriteRenderer _spriteRenderer, Vector3 _pos) {
        spriteRenderer = _spriteRenderer;
        pos = _pos;
    }
}

public class GameManager : MonoBehaviour
{
    //어떤 행위들을 이벤트로 제어할 수 있을까?
    //생각나는건... 한 블록을 선택했을때 나머지 블록들을 고정시켜두도록 한다던지,
    //1line이 완성됬을 때, 터질 블록들에게 메시지를 보낸다던지..
    private IDisposable subscription1;
    private IDisposable subscription2;
    private IDisposable subscription3;

    [SerializeField] private GameObject silhouette;
    
    void Start() {
        subscription1 = Block.OnDragEvent.Subscribe(BlockDragHandler);
        subscription2 = Block.StartDragEvent.Subscribe(StartDragHandler);
        subscription3 = Block.EndDragEvent.Subscribe(EndDragHandler);

    }

    public void StartDragHandler(InitialDrag initialDrag) {
        Debug.Log("VAR");
        GameObject g = new GameObject();
        g.transform.position = initialDrag.pos.ChangeOnlyZ(-8);
        g.name = "dumm";
        var h = g.AddComponent<SpriteRenderer>();
        h.sprite = initialDrag.spriteRenderer.sprite;
        h.color = initialDrag.spriteRenderer.color.HalfAlpha();
        h.drawMode = SpriteDrawMode.Tiled;
        h.size = initialDrag.spriteRenderer.size;
        silhouette = g;

    }
    
    public void BlockDragHandler(Vector3 pos) {
    }

    public void EndDragHandler(Unit x) {
        GameObject.Destroy(silhouette);
    }

}
