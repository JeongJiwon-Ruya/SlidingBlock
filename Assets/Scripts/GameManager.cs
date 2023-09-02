using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class InitialDrag {
		public Block block;
		public int blockSize;
    public SpriteRenderer spriteRenderer;
    public Vector3 pos;

    public InitialDrag(Block _block, SpriteRenderer _spriteRenderer, Vector3 _pos, int _blockSize) {
	    block = _block;
        spriteRenderer = _spriteRenderer;
        pos = _pos;
        blockSize = _blockSize;
    }
}

public class GameManager : MonoBehaviour {
	public BlockGenerator blockGenerator;
	
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

    private (int left, int right) LimitCalculator(int initialBlockCode) {
	    Debug.Log(initialBlockCode + " << ");
	    var current = new int[10];
	    var firstIndex = 0;
	    var lastIndex = 0;
	    for (int i = 0; i < 10; i++) {
		    for (int j = 0; j < 10; j++) {
			    if (blockGenerator.blockMatrix[i, j] == initialBlockCode) {
				    currentBlockExistLineIndex = i;
				    firstIndex = j;
				    for (int k = 0; k < 10; k++) {
					    current[k] = blockGenerator.blockMatrix[i, k];
				    }
				    lastIndex = j;
				    for (int l = j+1; l < 10; l++) {
					    if (blockGenerator.blockMatrix[i, l] != initialBlockCode) break;
					    lastIndex = l;
				    }
				    goto ZeroCalculate;
			    }
		    }
	    }
	    //양옆 0 있는지, 몇개인지 계산. 
	    ZeroCalculate :
	    var left = 0;
	    var right = 0;
	    for (int i = firstIndex - 1; i > -1; i--) {
		    if (current[i] == initialBlockCode) continue;
		    if (current[i] != 0) break;
		    left++;
	    }
	    for (int i = lastIndex + 1; i < 10; i++) {
		    if (current[i] == initialBlockCode) continue;
		    if (current[i] != 0) break;
		    right++;
	    }
	    Debug.Log($"left : {left} , right : {right}");
	    return (left, right);
    }

    private int currentBlockExistLineIndex;

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
        //initialDrag.block.SetDragLimit(LimitCalculator(testline,2));
        
        /*
         * 현재 블록의 코드를 읽고, BlockMatrix를 확인해서, 몇번째 라인에 있고, 옆에 0인 칸이 몇개가 있어서 좌우로 몇칸 움직일 수 있는지 계산해서,
         * 행렬값을 전달해야함. LimitCalculator로 계산한 뒤, SetDragLimit로 전달.
         */
        initialDrag.block.SetDragLimit(LimitCalculator(initialDrag.block.code));
    }
    
    public void BlockDragHandler(Vector3 pos) {
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

    public void EndDragHandler(Vector3 endPosition) {
        Destroy(silhouette);
        foreach (var variable in lines) {
            variable.SetActive(false);
        }

        if (tempInitialDrag.pos == endPosition) return; //이동이 없었을 경우.
        //있을 경우 하술.
        int distance = (int)(endPosition.x - tempInitialDrag.pos.x);
        Debug.Log(distance + " << ");
        
        //distance만큼 code블록을 이동.
        var movedLine = new int[10];

        if (0 < distance) {
	        for (int i = 9; i > -1; i--) {
		        if (blockGenerator.blockMatrix[currentBlockExistLineIndex, i] == tempInitialDrag.block.code) {
			        blockGenerator.blockMatrix[currentBlockExistLineIndex, i] = 0;
			        blockGenerator.blockMatrix[currentBlockExistLineIndex, i + distance] = tempInitialDrag.block.code;
		        }
	        }
        }
        else {
	        for (int i = 0; i < 10; i++) {
		        if (blockGenerator.blockMatrix[currentBlockExistLineIndex, i] == tempInitialDrag.block.code) {
			        blockGenerator.blockMatrix[currentBlockExistLineIndex, i] = 0;
			        blockGenerator.blockMatrix[currentBlockExistLineIndex, i + distance] = tempInitialDrag.block.code;
		        }
	        }
        }
        
        //이젠, 블록이 이동했으니, (중력 이벤트 실행 후, 터칠수 있는 라인 있는지 계산. 있으면 터치기.) 터지면 또 중력 이벤트 실행, 끝나면 터칠수 있는 라인 있는지 계산. 있으면 터치기
        StartCoroutine(blockGenerator.UpdateGravityAndExplosion(currentBlockExistLineIndex, tempInitialDrag.block.code));
    }

    private List<int> OddEvenCorrection(InitialDrag block, float blockPosX) {
        var result = new List<int>();
        var x = blockPosX;
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
