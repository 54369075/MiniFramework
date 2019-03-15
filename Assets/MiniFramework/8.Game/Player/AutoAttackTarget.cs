﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MiniFramework
{
    public class AutoAttackTarget : MonoBehaviour
    {
        public float AttackDistance;
        public float MaxSearchDistance;
        public string TargetTag;
        public GameObject CurTarget;
        public bool IsFollow;
        private PlayerMove playerMove;
        // Use this for initialization
        void Start()
        {
            playerMove = GetComponent<PlayerMove>();
        }
        void Update()
        {
            if (CurTarget == null)
            {
                SelectTarget();
            }
            if (CurTarget != null)
            {
                Vector3 targetDir = (CurTarget.transform.position - transform.position).normalized;
                targetDir.y = 0;
                playerMove.SetLookDir(targetDir);
                if (Vector3.Distance(transform.position, CurTarget.transform.position) < AttackDistance)
                {
                    playerMove.MoveDir = Vector3.zero;
                }
                else if (IsFollow)
                {
                    playerMove.MoveDir = targetDir;
                }
                else //超出攻击范围 不跟随
                {
                    playerMove.MoveDir = Vector3.zero;

                }
                if (Vector3.Distance(transform.position, CurTarget.transform.position) > MaxSearchDistance)
                {
                    CancelTarget();
                }
            }
        }
        [ContextMenu("SelectTarget")]
        public void SelectTarget()
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(TargetTag);
            foreach (var item in objs)
            {
                if (Vector3.Distance(item.transform.position, transform.position) < MaxSearchDistance)
                {
                    item.GetComponent<MeshRenderer>().material.color = Color.red;
                    CurTarget = item;
                    return;
                }
            }
        }
        public void CancelTarget()
        {
            CurTarget.GetComponent<MeshRenderer>().material.color = Color.white;
            CurTarget = null;
        }
    }
}