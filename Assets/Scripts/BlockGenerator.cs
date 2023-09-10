using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public class BlockGenerator : MonoBehaviour {

	class BlockMoveData {
		public Block block;
		public int distance;

		public BlockMoveData(Block _block, int _distance) {
			block = _block;
			distance = _distance;
		}
	}
	
	private int blockCode = 1;

	public Block[] block;
	/* 0. 블록의 현재 배치 정보를 행렬로 저장
	 * 1. 블록을 아래에서 위로 생성(랜덤하게 조합해서)
	 * 2. 블록 이동 이벤트가 발생하고, 종료될때마다 삭제할 라인이 있는지 계산해서 삭제하고 행렬 업데이트.
	 * 3. 행렬의 최상위 행에 값이 입력되면(= 화면 젤 윗줄에 블록이 도달하면) 게임오버 처리.
	 *
	 *
	 * 진행 순서
	 * 1. 새롭게 추가될 블록조합을 GenerateBlocks를 통해서 pick. & matrix화
	 * 1.5 실제 블록 생성하고, activeBlockList에 저장
	 * 2. 기존의 행렬을 한칸 위로 올림(A -> B)
	 * 2.5 전/ 후의 매트릭스 정보를 모두 갖고, 두 행렬간 알고리즘 사용해서 상승 정보 계산.(밑에 블럭이 생김에 따라 상승하는 블럭도 있고, 위치가 유지되는 블록도 있으니. 여기서 하강하는 블록은 없음)
	 * 3. 올리는 애니메이션 재생
	 * 6. 새로운 블록이 올라온 상태(B)에서 터지는 블록 계산
	 * 7. 터지는 블록이 있다면, 터지는 행(n)의 값을 없애고 n행 위의 행들의 중력 적용값을 행렬에 적용 (B -> C) 
	 * 8. 터지는 라인의 터지는 애니메이션 재생하면서, 중력이 반영되는 애니메이션 재생.
	 * 9. 6~8을 반복하며 6에서 터지는 블록이 없을때까지 반복
	 * 10. 블록이 up될때의 프로세스 종료.
	 */

	public int[,] blockMatrix = new int[10, 10];
	public List<Block> activeBlocksList = new(); //블록은 1차원 리스트에 저장해놔도 되지않나?

	private void Start() {
		BlockRandomGenerateList.Initialize();
		newLineDataStack = GenerateBlocks();
		StartCoroutine(InitializeNewBlock());
	}

	private IEnumerator InitializeNewBlock() {
		//1
		Debug.Log("Phase 1 Start");
		var newLineData = newLineDataStack;

		var pastMatrix = (int[,])blockMatrix.Clone();

		//1.5
		Debug.Log("Phase 1.5 Start");
		var risingBlocksData = new List<BlockMoveData>();
		var tempCodeIndex = 0;
		var tempUIIndex = 0;
		foreach (var t in newLineData.originalList) {
			if (t > 0) {//t : block or space's length
				//Debug.Log("t : " + t);
				var newBlock = Instantiate(block[t-1]);
				newBlock.code = newLineData.codeArrayList[tempCodeIndex];
				tempCodeIndex += t;
				// 0 0 1 1 1 0 0 0 2 2, -2,3,-3,2
				/*tempCodeIndex += t - 1; //하필 마지막값에서 code값을 뽑음...
				newBlock.code = newLineData.codeArrayList[tempCodeIndex];*/
				
				
				//Debug.Log(tempCodeIndex + " , " + t);
				var xPos = 0; // 0 ~ 9 사이에. 1이면 그대로 배치, 2 -> 0.5 3 -> 1, 4 -> 1.5 5 -> 2
				newBlock.transform.position = new Vector3(tempUIIndex + t * 0.5f - 0.5f, 0, -9);
				activeBlocksList.Add(newBlock);
				risingBlocksData.Add(new BlockMoveData(newBlock, 1));
			} else {
				tempCodeIndex += Mathf.Abs(t);
			}
			tempUIIndex += Mathf.Abs(t);
		}

		//2
		Debug.Log("Phase 2 Start");
		for (int row = 1; row < 9; row++) {
			Array.Copy(blockMatrix, (row + 1) * 10, blockMatrix, row * 10, 10);
		}

		for (int i = 0; i < 10; i++) {
			blockMatrix[9, i] = newLineData.codeArrayList[i];
		}

		//

		//2.5
		Debug.Log("Phase 2.5 Start");
		//전 / 후의 매트릭스 정보를 모두 갖고, 두 행렬간 알고리즘 사용해야함.
		//블록과 해당 블록이 올라갈 값을 모두 모아놓고 한번에 실행. 이 반복문은 올라간 값을 계산함
		string temps ="";
		foreach (var VARIABLE in activeBlocksList) {
			temps += VARIABLE.code + " ";
		}
		Debug.Log("Active Block List : "+temps);
		for (int i = 9; i > -1; i--) {
			for (int j = 0; j < 10; j++) {
				if (blockMatrix[i, j] == 0) continue;
				for (int k = i; k < 10; k++) { //k는 pastMatrix를 search. 그니까 아래로 내려가야함. 과거엔 아래에 있었을테니까.
					if (pastMatrix[k, j] == blockMatrix[i, j]) {
						var risingDistance = k - i; //if a == 0이면 필요없는 값. i는 현재 행, k는 과거 행
						if(risingDistance == 0) continue;
						//Debug.Log("FIND " + blockMatrix[i,j]);
						var targetBlock = activeBlocksList.First(x => x.code == blockMatrix[i, j]);
						if(risingBlocksData.Any(x => x.block.code == targetBlock.code)) continue;
						risingBlocksData.Add(new BlockMoveData(targetBlock, risingDistance));
						break;
					}
				}
			}
		}

		//3
		Debug.Log("Phase 3 Start");
		var test = "";
		foreach (var VARIABLE in risingBlocksData) {
			test += VARIABLE.block.code + "  ";
		}

		Debug.Log("rising Blocks Data : " + test);
		float time = 0f;
		float speed = 0.01f;
		while (time < 1f) {
			foreach (var variable in risingBlocksData) {
				variable.block.transform.Translate(new Vector3(0, variable.distance * speed, 0));
			}
			time += speed;
			yield return new WaitForSeconds(Time.deltaTime);
		}
		
		//============================중력 알고리즘 ================
		var pastMatrix0 = (int[,])blockMatrix.Clone();
			for (int i = 9; i > -1; i--) { //계산 최소화를 위해 i를 전체순회하지않음
				int firstIndex = 0;
				while(firstIndex < 10) { //이 while문은 한줄을 처리함.
					/*
					 * for문 한번에 끝나려면 행렬을 마지막에 업데이트 해야함
					 *
					 * 0. 0일때는 pass. 0이외일때만 탐색
					 * 1. '4'를 탐지한 후, '4'가 몇번째 열까지 있는지 확인
					 * 2. 첫번째'4', 마지막 '4'의 index값을 갖고 아래로 탐색 시작
					 * 3. 첫번째, 마지막 위치 값이 모두 0이면 아래로 진행.
					 * 4. 첫번째, 마지막 둘중 하나라도 0이 아니면 break.
					 * 5. 아래의 0을 찾았다면, 마지막 0의 위치로 4들을 이동시킴. 아니면 이동시키면서 아래로 진행해도됨. 이동시키면서 탐색하는게 좋을듯?
					 * 6. 다음 index는 4의 마지막 위치 다음부터 다시 진행
					 */
					if(blockMatrix[i,firstIndex] == 0) {
						//블록 없음.
						firstIndex++;
						continue;
					}
					//블록이 존재.
					var currentBlockCode = blockMatrix[i, firstIndex];
					int lastIndex = firstIndex;
					while (true) { //어디까지 현재 블록이 있는지 탐색
						if (lastIndex == 10 || blockMatrix[i, lastIndex] != currentBlockCode) {
							lastIndex--;
							break;
						}
						lastIndex++;
					}
					
					for (int j = i+1; j < 10; j++) { //한칸 밑부터 아래로 검색
						if (blockMatrix[j, firstIndex] == 0 && blockMatrix[j, lastIndex] == 0) { //밑에가 0이면,
							for (int k = firstIndex; k <= lastIndex; k++) { //한칸 아래로 내리기
								blockMatrix[j-1, k] = 0;
								blockMatrix[j, k] = currentBlockCode;
							} continue;
						} break; //더 내려갈게 없다면, 이 블록에 대해 상황 종료.
					}
					firstIndex = lastIndex + 1;
				}
			}
			//======================================7 종료)))))
			Debug.Log("Phase 8 Start");
			//8. 터지는 라인의 터지는 애니메이션 재생하면서, 중력이 반영되는 애니메이션 재생.
			//터지는 애니메이션
			var fallingBlocksData0 = new List<BlockMoveData>();
			for (int i = 9; i > 0; i--) {
				for (int j = 0; j < 9; j++) {
					if (blockMatrix[i, j] == 0) continue;
					for (int k = i; k > 0; k--) { //과거엔 현재보다 블록들이 더 위에 있었을것.
						if (pastMatrix0[k, j] == blockMatrix[i, j]) {
							var fallingDistance = i - k; //if a == 0이면 필요없는 값. i는 현재 행, k는 과거 행
							if(fallingDistance == 0) continue;
							var targetBlock = activeBlocksList.First(x => x.code == blockMatrix[i, j]);
							if(fallingBlocksData0.Any(x => x.block.code == targetBlock.code)) continue;
							fallingBlocksData0.Add(new BlockMoveData(targetBlock, fallingDistance));
							break;
						}
					}
				}
			}
			
			time = 0f;
			speed = 0.01f;
			while (time < 1f) {
				foreach (var variable in fallingBlocksData0) {
					variable.block.transform.Translate(new Vector3(0, -variable.distance * speed, 0));
				}
				time += speed;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			//============================중력 알고리즘 END================
		
		
		
		Search:
		
		//6
		Debug.Log("Phase 6 Start");
		var destroyBlocks = new List<Block>();
		List<int> destroyLines = new List<int>(); //아래에 위치한 라인(index 9부터 순회)부터 쌓인, 삭제될  행 값
		for (int i = 9; i > -1; i--) {
			var a = Enumerable.Range(0, 10).Select(j => blockMatrix[i, j]).ToArray();
			if (a.All(x => x != 0)) {
				/*
				 * [0,0,0,1,1,1,1,2,2,0]의 blockMatrix에서 1,2만 뽑아오고, activeBlockList에서 code가 1,2인 블록만 갖고와야됨.
				 */
				destroyLines.Add(i);
				var refineCode = a.Distinct();
				var findDestroyBlocks =
				from code in refineCode 
					from blk in activeBlocksList
						where blk.code == code
						select blk;
				destroyBlocks.AddRange(findDestroyBlocks);
			}
		}

		if (destroyLines.Count != 0) { //새로운 블록이 아래에서 올라왔을때 터지는 라인이 있는 경우
			Debug.Log("Phase 7 Start");
			//7
			/*
			 * blockMatrix의 destroyLines값들을 모두 0으로 변경하고, 그 뒤에 중력알고리즘을 돌리기.
			 * 값을 0으로 변경하면서 activeBlockList도 정리... 정리는 애니메이션 재생된 이후에 해야될듯.(nullReference 에러 뜰듯) 아니 destroyBlocks로 옮겼으니까 상관없으려나? => Ienumerable말고 List로 캐싱해놓으면 됨(lazy evaluation 회피) => 여기서 정리해도됨. 
			 */
			foreach (var blk in destroyBlocks) {
				activeBlocksList.Remove(blk);
			}

			var pastMatrix2 = (int[,])blockMatrix.Clone(); //필요없을거같음. 필요하네. 애니메이션
			foreach (var t in destroyLines) {
				for (int i = 0; i < 10; i++) {
					blockMatrix[t, i] = 0;
				}
			}

			for (int i = 9; i > -1; i--) { //계산 최소화를 위해 i를 전체순회하지않음
				int firstIndex = 0;
				while(firstIndex < 10) { //이 while문은 한줄을 처리함.
					/*
					 * for문 한번에 끝나려면 행렬을 마지막에 업데이트 해야함
					 *
					 * 0. 0일때는 pass. 0이외일때만 탐색
					 * 1. '4'를 탐지한 후, '4'가 몇번째 열까지 있는지 확인
					 * 2. 첫번째'4', 마지막 '4'의 index값을 갖고 아래로 탐색 시작
					 * 3. 첫번째, 마지막 위치 값이 모두 0이면 아래로 진행.
					 * 4. 첫번째, 마지막 둘중 하나라도 0이 아니면 break.
					 * 5. 아래의 0을 찾았다면, 마지막 0의 위치로 4들을 이동시킴. 아니면 이동시키면서 아래로 진행해도됨. 이동시키면서 탐색하는게 좋을듯?
					 * 6. 다음 index는 4의 마지막 위치 다음부터 다시 진행
					 */
					if(blockMatrix[i,firstIndex] == 0) {
						//블록 없음.
						firstIndex++;
						continue;
					}
					//블록이 존재.
					var currentBlockCode = blockMatrix[i, firstIndex];
					int lastIndex = firstIndex;
					while (true) { //어디까지 현재 블록이 있는지 탐색
						if (lastIndex == 10 || blockMatrix[i, lastIndex] != currentBlockCode) {
							lastIndex--;
							break;
						}
						lastIndex++;
					}
					
					for (int j = i+1; j < 10; j++) { //한칸 밑부터 아래로 검색
						if (blockMatrix[j, firstIndex] == 0 && blockMatrix[j, lastIndex] == 0) { //밑에가 0이면,
							for (int k = firstIndex; k <= lastIndex; k++) { //한칸 아래로 내리기
								blockMatrix[j-1, k] = 0;
								blockMatrix[j, k] = currentBlockCode;
							} continue;
						} break; //더 내려갈게 없다면, 이 블록에 대해 상황 종료.
					}
					firstIndex = lastIndex + 1;
				}
			}
			//======================================7 종료
			Debug.Log("Phase 8 Start");
			//8. 터지는 라인의 터지는 애니메이션 재생하면서, 중력이 반영되는 애니메이션 재생.
			//터지는 애니메이션
			foreach (var blk in destroyBlocks) {
				//* blk.SetDestroyAnimation(destroy)뭐 이런식으로 하고.. 거기서 destroy하는걸로 처리
				blk.DestroyAnimation();
			}
			var fallingBlocksData = new List<BlockMoveData>();
			for (int i = 9; i > 0; i--) {
				for (int j = 0; j < 9; j++) {
					if (blockMatrix[i, j] == 0) continue;
					for (int k = i; k > 0; k--) { //과거엔 현재보다 블록들이 더 위에 있었을것.
						if (pastMatrix2[k, j] == blockMatrix[i, j]) {
							var fallingDistance = i - k; //if a == 0이면 필요없는 값. i는 현재 행, k는 과거 행
							if(fallingDistance == 0) continue;
							var targetBlock = activeBlocksList.First(x => x.code == blockMatrix[i, j]);
							if(fallingBlocksData.Any(x => x.block.code == targetBlock.code)) continue;
							fallingBlocksData.Add(new BlockMoveData(targetBlock, fallingDistance));
							break;
						}
					}
				}
			}
			//떨어지는 애니메이션 재생.
			time = 0f;
			speed = 0.01f;
			while (time < 1f) {
				foreach (var variable in fallingBlocksData) {
					variable.block.transform.Translate(new Vector3(0, -variable.distance * speed, 0));
				}
				time += speed;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			goto Search;
		}
		
		
		//새로운 블록이 아래에서 올라왔을때, 터지는 라인이 없는 경우
		Debug.Log("상황 종료");
		var temp = "";
		for (int i = 0; i < 10; i++) {
			for (int j = 0; j < 10; j++) {
				temp += blockMatrix[i, j] + " ";
			}
			temp += "\n";
		}

		Debug.Log(temp);
		newLineDataStack = GenerateBlocks();
		var tempStack = "";
		foreach (var VARIABLE in newLineDataStack.Item2) {
			tempStack += VARIABLE + "  ";
		}

		Debug.Log("nextLine : " + tempStack);
	}

	public (List<int> originalList, int[] codeArrayList) newLineDataStack;
	
	private (List<int> originalList, int[] codeArrayList) GenerateBlocks() {
        var current = BlockRandomGenerateList.GetActive10List();
        var result = new int[10];
        
        var index = 0;
        foreach (var variable in current) {
            //Debug.Log(variable > 0 ? $"블록 : {variable}" : $"빈칸 : {variable}");
            var tempBlockCode = blockCode++;
            var max = index;
	            for (int i = index; i < max + Mathf.Abs(variable); i++) {
		            result[index] = variable > 0 ? tempBlockCode : 0;
		            index++;
	            }
        }
        /*
         * current : -3, 4, 2, -1
         * result : [0,0,0,1,1,1,1,2,2,0]
         */
        string s = "";
        foreach (var VARIABLE in result) {
	        s += VARIABLE + " ";
        }

//        Debug.Log("result : " + s);
        
        return (current, result); 
    }
	
	public IEnumerator UpdateGravityAndExplosion(int existLineIndex, int blockCode) {
		//이젠, 블록의 좌우이동이 행렬에 적용된 상태.
		/* 0. 이동한 블록이 떨어질 수 있는지 확인
		 * 1. 중력 애니메이션 이벤트 실행
		 * 2. 터칠수 있는 라인 계산
		 * 3. 있으면 터치고 1로 돌아감.
		 * 4. 없으면 종료.
		 */
		var pastMatrix1 = (int[,])blockMatrix.Clone();
		for (int i = 9; i > -1; i--) { //계산 최소화를 위해 i를 전체순회하지않음
			int firstIndex = 0;
			while(firstIndex < 10) { //이 while문은 한줄을 처리함.
				/*
				 * for문 한번에 끝나려면 행렬을 마지막에 업데이트 해야함
				 *
				 * 0. 0일때는 pass. 0이외일때만 탐색
				 * 1. '4'를 탐지한 후, '4'가 몇번째 열까지 있는지 확인
				 * 2. 첫번째'4', 마지막 '4'의 index값을 갖고 아래로 탐색 시작
				 * 3. 첫번째, 마지막 위치 값이 모두 0이면 아래로 진행.
				 * 4. 첫번째, 마지막 둘중 하나라도 0이 아니면 break.
				 * 5. 아래의 0을 찾았다면, 마지막 0의 위치로 4들을 이동시킴. 아니면 이동시키면서 아래로 진행해도됨. 이동시키면서 탐색하는게 좋을듯?
				 * 6. 다음 index는 4의 마지막 위치 다음부터 다시 진행
				 */

				
				if(blockMatrix[i,firstIndex] == 0) {
					//블록 없음.
					firstIndex++;
					continue;
				}
				//블록이 존재.
				var currentBlockCode = blockMatrix[i, firstIndex];
				int lastIndex = firstIndex;
				while (true) { //어디까지 현재 블록이 있는지 탐색
					if (lastIndex == 10 || blockMatrix[i, lastIndex] != currentBlockCode) {
						lastIndex--;
						break;
					}
					lastIndex++;
				}
					
				for (int j = i+1; j < 10; j++) { //한칸 밑부터 아래로 검색
					if (blockMatrix[j, firstIndex] == 0 && blockMatrix[j, lastIndex] == 0) { //밑에가 0이면,
						for (int k = firstIndex; k <= lastIndex; k++) { //한칸 아래로 내리기
							blockMatrix[j-1, k] = 0;
							blockMatrix[j, k] = currentBlockCode;
						} continue;
					} break; //더 내려갈게 없다면, 이 블록에 대해 상황 종료.
				}
				firstIndex = lastIndex + 1;
			}
		}
		
		var fallingBlocksData1 = new List<BlockMoveData>();
		for (int i = 9; i > 0; i--) {
			for (int j = 0; j < 9; j++) {
				if (blockMatrix[i, j] == 0) continue;
				for (int k = i; k > 0; k--) { //과거엔 현재보다 블록들이 더 위에 있었을것.
					if (pastMatrix1[k, j] == blockMatrix[i, j]) {
						var fallingDistance = i - k; //if a == 0이면 필요없는 값. i는 현재 행, k는 과거 행
						if(fallingDistance == 0) continue;
						var targetBlock = activeBlocksList.First(x => x.code == blockMatrix[i, j]);
						if(fallingBlocksData1.Any(x => x.block.code == targetBlock.code)) continue;
						fallingBlocksData1.Add(new BlockMoveData(targetBlock, fallingDistance));
						break;
					}
				}
			}
		}
		//떨어지는 애니메이션 재생.
		float time = 0f;
		float speed = 0.01f;
		while (time < 1f) {
			foreach (var variable in fallingBlocksData1) {
				Debug.Log(variable.block.code + " falling");
				variable.block.transform.Translate(new Vector3(0, -variable.distance * speed, 0));
			}
			time += speed;
			yield return new WaitForSeconds(Time.deltaTime);
		}
		
		
		
		//====================================================================================이부분은 start에서도 사용하고, 코루틴으로 해야되서, 함수 하나 파야할듯.
		/*
		 * 터칠수 있는 라인 계산 ~ 진행
		 */
		Search :
		var destroyBlocks = new List<Block>();
		List<int> destroyLines = new List<int>(); //아래에 위치한 라인(index 9부터 순회)부터 쌓인, 삭제될  행 값
		for (int i = 9; i > -1; i--) {
			var a = Enumerable.Range(0, 10).Select(j => blockMatrix[i, j]).ToArray();
			if (a.All(x => x != 0)) {
				/*
				 * [0,0,0,1,1,1,1,2,2,0]의 blockMatrix에서 1,2만 뽑아오고, activeBlockList에서 code가 1,2인 블록만 갖고와야됨.
				 */
				destroyLines.Add(i);
				var refineCode = a.Distinct();
				var findDestroyBlocks =
				from code in refineCode 
				from blk in activeBlocksList
				where blk.code == code
				select blk;
				destroyBlocks.AddRange(findDestroyBlocks);
			}
		}
		
		
		
		if (destroyLines.Count != 0) { //새로운 블록이 아래에서 올라왔을때 터지는 라인이 있는 경우
			//7
			/*
			 * blockMatrix의 destroyLines값들을 모두 0으로 변경하고, 그 뒤에 중력알고리즘을 돌리기.
			 * 값을 0으로 변경하면서 activeBlockList도 정리... 정리는 애니메이션 재생된 이후에 해야될듯.(nullReference 에러 뜰듯) 아니 destroyBlocks로 옮겼으니까 상관없으려나? => Ienumerable말고 List로 캐싱해놓으면 됨(lazy evaluation 회피) => 여기서 정리해도됨. 
			 */
			foreach (var blk in destroyBlocks) {
				activeBlocksList.Remove(blk);
			}

			var pastMatrix2 = (int[,])blockMatrix.Clone(); //필요없을거같음. 필요하네. 애니메이션
			foreach (var t in destroyLines) {
				for (int i = 0; i < 10; i++) {
					blockMatrix[t, i] = 0;
				}
			}

			for (int i = destroyLines[0]-1; i > -1; i--) { //계산 최소화를 위해 i를 전체순회하지않음
				int firstIndex = 0;
				while(firstIndex < 10) { //이 while문은 한줄을 처리함.
					/*
					 * for문 한번에 끝나려면 행렬을 마지막에 업데이트 해야함
					 *
					 * 0. 0일때는 pass. 0이외일때만 탐색
					 * 1. '4'를 탐지한 후, '4'가 몇번째 열까지 있는지 확인
					 * 2. 첫번째'4', 마지막 '4'의 index값을 갖고 아래로 탐색 시작
					 * 3. 첫번째, 마지막 위치 값이 모두 0이면 아래로 진행.
					 * 4. 첫번째, 마지막 둘중 하나라도 0이 아니면 break.
					 * 5. 아래의 0을 찾았다면, 마지막 0의 위치로 4들을 이동시킴. 아니면 이동시키면서 아래로 진행해도됨. 이동시키면서 탐색하는게 좋을듯?
					 * 6. 다음 index는 4의 마지막 위치 다음부터 다시 진행
					 */
					if(blockMatrix[i,firstIndex] == 0) {
						//블록 없음.
						firstIndex++;
						continue;
					}
					//블록이 존재.
					var currentBlockCode = blockMatrix[i, firstIndex];
					int lastIndex = firstIndex;
					while (true) { //어디까지 현재 블록이 있는지 탐색
						if (lastIndex == 10 || blockMatrix[i, lastIndex] != currentBlockCode) {
							lastIndex--;
							break;
						}
						lastIndex++;
					}
					
					for (int j = i+1; j < 10; j++) { //한칸 밑부터 아래로 검색
						if (blockMatrix[j, firstIndex] == 0 && blockMatrix[j, lastIndex] == 0) { //밑에가 0이면,
							for (int k = firstIndex; k <= lastIndex; k++) { //한칸 아래로 내리기
								blockMatrix[j-1, k] = 0;
								blockMatrix[j, k] = currentBlockCode;
							} continue;
						} break; //더 내려갈게 없다면, 이 블록에 대해 상황 종료.
					}
					firstIndex = lastIndex + 1;
				}
			}
			//======================================7 종료
			
			//8. 터지는 라인의 터지는 애니메이션 재생하면서, 중력이 반영되는 애니메이션 재생.
			//터지는 애니메이션
			foreach (var blk in destroyBlocks) {
				//* blk.SetDestroyAnimation(destroy)뭐 이런식으로 하고.. 거기서 destroy하는걸로 처리
				blk.DestroyAnimation();
			}
			var fallingBlocksData = new List<BlockMoveData>(); //움직임으로 터졌을때 여기서 처리
			for (int i = 9; i > 0; i--) {
				for (int j = 0; j < 9; j++) {
					if (blockMatrix[i, j] == 0) continue;
					for (int k = i; k > 0; k--) { //과거엔 현재보다 블록들이 더 위에 있었을것.
						if (pastMatrix2[k, j] == blockMatrix[i, j]) {
							var fallingDistance = i - k; //if a == 0이면 필요없는 값. i는 현재 행, k는 과거 행
							if(fallingDistance == 0) continue;
							var targetBlock = activeBlocksList.First(x => x.code == blockMatrix[i, j]);
							if(fallingBlocksData.Any(x => x.block.code == targetBlock.code)) continue;
							fallingBlocksData.Add(new BlockMoveData(targetBlock, fallingDistance));
							break;
						}
					}
				}
			}
			//떨어지는 애니메이션 재생.
			time = 0f;
			speed = 0.01f;
			while (time < 1f) {
				foreach (var variable in fallingBlocksData) {
					variable.block.transform.Translate(new Vector3(0, -variable.distance * speed, 0));
				}
				time += speed;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			goto Search;
		}
		
		
		//새로운 블록이 아래에서 올라왔을때, 터지는 라인이 없는 경우
		Debug.Log("상황 종료");
		var temp = "";
		for (int i = 0; i < 10; i++) {
			for (int j = 0; j < 10; j++) {
				temp += blockMatrix[i, j] + " ";
			}
			temp += "\n";
		}

		Debug.Log(temp);
		//====================================================================================이부분은 start에서도 사용하고, 코루틴으로 해야되서, 함수 하나 파야할듯. END

		StartCoroutine(InitializeNewBlock());
	}
}
