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
    public List<int> activeLines;

    private InitialDrag tempInitialDrag;

    void Start() {
        subscription1 = Block.OnDragEvent.Subscribe(BlockDragHandler);
        subscription2 = Block.StartDragEvent.Subscribe(StartDragHandler);
        subscription3 = Block.EndDragEvent.Subscribe(EndDragHandler);
    }

    private void StartDragHandler(InitialDrag initialDrag) {
        tempInitialDrag = initialDrag;
        //선택한 블록 길이만큼 경계선 칠해주기
        activeLines = OddEvenCorrection(tempInitialDrag, tempInitialDrag.pos.x);
        
        activeLines.ForEach(index => lines[index].SetActive(true));

        var g = new GameObject();
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
        Debug.Log(pos);
        if(tempInitialDrag == null) return;
        var list = OddEvenCorrection(tempInitialDrag, pos.x);
        if (list.SequenceEqual(activeLines)) return;
        activeLines.ForEach(index => lines[index].SetActive(false));
        activeLines = list.ToList();
        activeLines.ForEach(index => lines[index].SetActive(true));
        //list의 값들과 activeLines값 비교.
        /*
         * 좌 또는 우로 1씩 움질일때마다 이동함.
         * 짝수는 0.5씩 이동해서 보정해줘야함. 홀수는 1씩 이동
         * 기준은 현재 경계선이 있는 위치(lines List로 확인 가능)
         * list를 좌표로 변환한 다음. pos의 x값과 비교함. 비교해서 0.5이상 움직이면 lines값을 옮기고. 0.5이하로 진행되는 이동이면 return
         */
        //현재 블록 위치가 기준점 기준으로 좌 또는 우로 얼마나 움직였는지 판단하여 경계선 칠해주는거 변경.
    }

    public void EndDragHandler(Unit x) {
        GameObject.Destroy(silhouette);
        foreach (var variable in lines) {
            variable.SetActive(false);
        }
    }

    private List<int> OddEvenCorrection(InitialDrag block, float blockPosX) {
        var result = new List<int>();
        var x = blockPosX + 4;
        if (block.blockSize % 2 == 0) { //짝수 => x가 .5 임
            x -= (block.blockSize / 2);
            for (int i = 0; i < block.blockSize; i++) {
                result.Add((int)Mathf.Ceil(x));
                x++;
            }
        }
        else { //홀수 => x가 정수임. 1,3,5
            for (int i = (int)x - (block.blockSize / 2); i <= x + (block.blockSize/2); i++) {
                result.Add(i);
            }
        }
        return result;
    }

}
