using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class test : MonoBehaviour {
	public List<GameObject> triangles;
    // Start is called before the first frame update
    void Start() {
	    var a = triangles.Where(x => x.name == "Triangle").ToList();
	    triangles.RemoveAt(0);
	    foreach (var VARIABLE in a) {
		    Debug.Log(VARIABLE.name);
	    }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
