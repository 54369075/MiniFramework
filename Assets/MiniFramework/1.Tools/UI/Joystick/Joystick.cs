﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniFramework
{
    /// <summary>
    /// 摇杆
    /// </summary>
    public class Joystick : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public RectTransform Rocker;//摇杆
        public RectTransform Base;//底座
        public RectTransform Arrow;//摇杆箭头
        public float MaxDistance;//摇杆最大移动距离
        public bool IsAutoHide;//是否自动消失
        public Action OnBeginDragHandler;//摇杆开始拖拽事件
        public Action<Vector2> OnDragHandler;//摇杆拖拽中事件
        public Action OnEndDragHandler;//摇杆结束拖拽事件

        private Vector2 rockerPos;
        private int pointerID = -1;
        //摇杆方向
        public Vector2 RockerDir
        {
            get { return Rocker.localPosition.normalized; }
        }
        void Start()
        {
            if (IsAutoHide)
            {
                Base.gameObject.SetActive(false);
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            //避免其他触摸影响
            if (pointerID == -1)
            {
                pointerID = eventData.pointerId;
            }
            else
            {
                return;
            }

            if (IsAutoHide)
            {
                Base.gameObject.SetActive(true);

                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.enterEventCamera, out position);
                Base.localPosition = position;
            }

            if (OnBeginDragHandler != null)
            {
                OnBeginDragHandler();
            }

            rockerPos = Vector2.zero;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerID)
            {
                return;
            }
            rockerPos += eventData.delta;
            float distance = Vector2.Distance(rockerPos, Vector2.zero);
            if (distance > MaxDistance)
            {
                rockerPos = (MaxDistance / distance) * rockerPos;
            }
            Rocker.localPosition = rockerPos;
            if (Arrow != null)
            {
                Arrow.localPosition = Rocker.localPosition.normalized * MaxDistance;
                Arrow.up = Rocker.localPosition.normalized;
            }
            if (OnDragHandler != null)
            {
                OnDragHandler(Rocker.localPosition.normalized);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerID)
            {
                return;
            }
            Rocker.localPosition = Vector3.zero;
            if (OnEndDragHandler != null)
            {
                OnEndDragHandler();
            }
            if (IsAutoHide)
            {
                Base.gameObject.SetActive(false);
            }
            pointerID = -1;
        }
    }
}