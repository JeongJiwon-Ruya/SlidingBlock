using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class InnerCollider : MonoBehaviour
{
    [SerializeField] private Collider2D col;
    public bool isLeft;
    void Start()
    {
        col = GetComponent<Collider2D>();
        var parent = transform.parent.GetComponent<Block>();

        this.OnTriggerEnter2DAsObservable()
            .Where(x => (x.gameObject.name != this.name) && x.gameObject.CompareTag("Wall") || x.gameObject.CompareTag("Block"))
            .Subscribe(x => { parent.ChangeBlockedState(x.gameObject, isLeft || isLeft, true); });
        
        this.OnTriggerExit2DAsObservable()
            .Where(x => (x.gameObject.name != this.name) && x.gameObject.CompareTag("Wall") || x.gameObject.CompareTag("Block"))
            .Subscribe(x => { parent.ChangeBlockedState(x.gameObject, isLeft || isLeft, false); });
    }

}

