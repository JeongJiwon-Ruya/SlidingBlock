using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BlockRandomGenerateList{
    // 1부터 5까지의 숫자
    private static readonly int[] numbers = { 1, 2, 3, 4 };
    private static IEnumerable<IEnumerable<int>> combinations;
    private static List<List<int>> sum10List;

    public static void Initialize() {
        sum10List = new List<List<int>>();
        for (int i = 3; i < 10; i++) {
            combinations = GetCombinationsWithReplacement(numbers, i);
            
            // 각 조합에 대해 합이 10인 경우만 출력
            foreach (IEnumerable<int> combination in combinations) {
                if (combination.Sum() == 10 && (combination.Count(item => item == 1) < 10)) {
                    sum10List.Add(combination.ToList());
//                    Debug.Log(string.Join(" + ", combination) + " = 10");
                }
            }
        }
    }

    private static IEnumerable<IEnumerable<T>> GetCombinationsWithReplacement<T>(IEnumerable<T> list, int length) {
        if (length == 1) {
            return list.Select(x => new T[] {x});
        } else {
            return GetCombinationsWithReplacement(list, length - 1)
                .SelectMany(x => list, (x, y) => x.Concat(new T[] {y}));
        }
    }

    public static List<int> GetActive10List() {
        //가장 큰 인자는 무조건 블록으로 생성.
        //자잘한 인자들은 랜덤하게 블록 또는 공백으로 생성되도록.
        var selectList = sum10List.OrderBy(x => Guid.NewGuid()).First();
        var maxValue = selectList.Max();
        var minValue = selectList.Min();
        for (int i = 0; i < selectList.Count; i++) {
            if (selectList[i] == maxValue) continue;
            if (selectList[i] == minValue) {
                selectList[i] = -selectList[i];
            } else {
                var random = new System.Random();
                var sign = random.Next(2);
                selectList[i] = sign == 0 ? -selectList[i] : selectList[i];
            }

        }
        return selectList;
    }
}