using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGenerator : MonoBehaviour
{
    /* 0. 블록의 현재 배치 정보를 행렬로 저장
     * 1. 블록을 아래에서 위로 생성(랜덤하게 조합해서)
     * 2. 블록 이동 이벤트가 발생하고, 종료될때마다 삭제할 라인이 있는지 계산해서 삭제하고 행렬 업데이트.
     * 3. 행렬의 최상위 행에 값이 입력되면(= 화면 젤 윗줄에 블록이 도달하면) 게임오버 처리.
     *
     *
     * 진행 순서
     * 1. 새롭게 추가될 블록조합을 GenerateBlocks를 통해서 pick. & matrix화
     * 2. 기존의 행렬을 한칸 위로 올림(A -> B)
     * 3. 올리는 애니메이션 재생
     * 4. 올린 상태(B)에서의 중력반영여부 계산(B -> B')
     * 5. 중력이 반영되는 애니메이션 재생. 여기서 변화가 없다면 애니메이션 재생 안하면 절약될듯
     * 6. 중력이 반영된 상태(B')에서 터지는 블록 계산
     * 7. 터지는 블록이 있다면, 터지는 행(n)의 값을 없애고 n행 위의 행들의 중력 적용값을 행렬에 적용 (B' -> C) 
     * 8. 터지는 라인의 터지는 애니메이션 재생하면서, 중력이 반영되는 애니메이션 재생.
     * 9. 6~8을 반복하며 6에서 터지는 블록이 없을때까지 반복
     * 10. 블록이 up될때의 프로세스 종료.
     */

    private int[,] blockMatrix = new int[10, 10];

    private void Start() {
        BlockRandomGenerateList.Initialize();
        GenerateBlocks();
    }

    private void GenerateBlocks() {
        var current = BlockRandomGenerateList.GetActive10List();

        foreach (var variable in current) {
            Debug.Log(variable > 0 ? $"블록 : {variable}" : $"빈칸 : {variable}");
        }
        /*
         * 1. 0,1,2,3,4
         */
    }
    
    
}
