using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGenerator : MonoBehaviour
{
    /* 0. 블록의 현재 배치 정보를 행렬로 저장
     * 1. 블록을 아래에서 위로 생성(랜덤하게 조합해서)
     * 2. 블록 이동 이벤트가 발생하고, 종료될때마다 삭제할 라인이 있는지 계산해서 삭제하고 행렬 업데이트.
     * 3. 행렬의 최상위 행에 값이 입력되면(= 화면 젤 윗줄에 블록이 도달하면) 게임오버 처리.
     */
}
