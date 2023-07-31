using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGenerator : MonoBehaviour {
    private int[,] blockMatrix = new int[10, 10]; 
    public GameObject block1, block2, block3, block4, block5;
	/* 0. 블록의 현재 배치 정보를 행렬로 저장
	 * 1. 블록을 아래에서 위로 생성(랜덤하게 조합해서)
	 * 2. 블록 이동 이벤트가 발생하고, 종료될때마다 삭제할 라인이 있는지 계산해서 삭제하고 행렬 업데이트.
	 * 3. 행렬의 최상위 행에 값이 입력되면(= 화면 젤 윗줄에 블록이 도달하면) 게임오버 처리.
	 */


    private void Start() {
        BlockRandomGenerateList.Initialize();
        GenerateBlocks();
    }

    private void GenerateBlocks() {
        var current = BlockRandomGenerateList.GetActive10List();

        var basePos = 0;
        foreach (var variable in current) {
            Debug.Log(variable > 0 ? $"블록 : {variable}" : $"빈칸 : {variable}");
            if (variable > 0) {
	            var a = GameObject.Instantiate(IndexChanger(variable));
	            var ab = a.GetComponent<Block>();
	            //-4 ~ 5사이로 채워넣어야함 -> 1 ~ 10으로 계산하고 전체 -5하면됨
	            var c = basePos + CalculateX(variable) - 5;
	            a.transform.position = new Vector3(basePos + CalculateX(variable) - 5, -8f, -9f);
	            Debug.Log(c);
	            basePos += variable;
            }
            else {
	            basePos -= variable;
            }
        }
        /*
         * 1. 0,1,2,3,4
         */
    }

    private GameObject IndexChanger(int index) {
	    return index switch {
	    1 => block1,
	    2 => block2,
	    3 => block3,
	    4 => block4,
	    5 => block5,
	    _ => block1
	    };
    }

    private float CalculateX(int blockSize) {
	    //even
	    if (blockSize % 2 == 0) return blockSize *0.5f + 0.5f;
	    //odd
	    return blockSize * 0.5f + 0.5f;
    }
}
