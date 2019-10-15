﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniFramework;
using UnityEngine.UI;
using System;
using System.Net;

public class TestClient : MonoBehaviour
{
    public InputField Content;
    public Text Text;
    public Button Send;
    public Button Close;
    // Use this for initialization
    void Start()
    {
        SocketManager.Instance.MiniTcpClient.Launch("127.0.0.1", 8888);
        Send.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(Content.text))
            {
                SocketManager.Instance.MiniTcpClient.Send(MsgID.Test, null);
            }
            else
            {
                SocketManager.Instance.MiniTcpClient.Send(MsgID.Test, SerializeUtil.ToProtoBuff(Content.text));
            }
            Text.text = DateTime.Now + ":" + Content.text;
            Text.color = Color.black;
            Instantiate(Text.gameObject, Text.transform.parent).SetActive(true);
        });
        Close.onClick.AddListener(() =>
        {
            SocketManager.Instance.MiniTcpClient.Close();
        });
        MsgDispatcher.Instance.Regist(this, MsgID.Test, (s) =>
        {
            string txt = SerializeUtil.FromProtoBuff<string>(s);
            Text.text = DateTime.Now + ":" + txt;
            Text.color = Color.blue;
            Instantiate(Text.gameObject, Text.transform.parent).SetActive(true);
        });
    }
    private void Update()
    {
        if (SocketManager.Instance.MiniTcpClient.IsConnected)
        {
            // MiniTcpClient.Instance.Send(MsgID.Test,SerializeUtil.ToProtoBuff("123213nkasjnakdad123132ansklncasdl123嘻嘻嘻嘻嘻嘻嘻嘻寻寻寻寻寻寻寻寻寻寻寻"));
        }
    }

}
