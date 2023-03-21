using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class InitialDrag {
    public int blockSize;
    public SpriteRenderer spriteRenderer;
    public Vector3 pos;

    public InitialDrag(SpriteRenderer _spriteRenderer, Vector3 _pos, int _blockSize) {
        spriteRenderer = _spriteRenderer;
        pos = _pos;
        blockSize = _blockSize;
    }
}

public class GameManager : MonoBehaviour
{
    //어떤 행위들을 이벤트로 제어할 수 있을까?
    //생각나는건... 한 블록을 선택했을때 나머지 블록들을 고정시켜두도록 한다던지,
    //1line이 완성됬을 때, 터질 블록들에게 메시지를 보낸다던지..
    private IDisposable subscription1, subscription2, subscription3;

    [SerializeField] private GameObject silhouette;
    public List<GameObject> lines;

    void Start() {
        subscription1 = Block.OnDragEvent.Subscribe(BlockDragHandler);
        subscription2 = Block.StartDragEvent.Subscribe(StartDragHandler);
        subscription3 = Block.EndDragEvent.Subscribe(EndDragHandler);

    }

    public void StartDragHandler(InitialDrag initialDrag) {
        //선택한 블록 길이만큼 경계선 칠해주기
        var result = new List<int>();
        var x = initialDrag.pos.x + 4;
        if (initialDrag.blockSize % 2 == 0) { //짝수 => x가 .5 임
            x -= (initialDrag.blockSize / 2);
            for (int i = 0; i < initialDrag.blockSize; i++) {
                result.Add((int)Mathf.Ceil(x));
                x++;
            }
        }
        else { //홀수 => x가 정수임. 1,3,5
            for (int i = (int)x - (initialDrag.blockSize / 2); i <= x + (initialDrag.blockSize/2); i++) {
                result.Add(i);
            }
        }

        result.ForEach(index => lines[index].SetActive(true));
        
        
        
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
        //현재 블록 위치가 기준점 기준으로 좌 또는 우로 얼마나 움직였는지 판단하여 경계선 칠해주는거 변경.
    }

    public void EndDragHandler(Unit x) {
        GameObject.Destroy(silhouette);
        foreach (var variable in lines) {
            variable.SetActive(false);
        }
    }

}
