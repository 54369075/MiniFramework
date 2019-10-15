﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniFramework;
using System.Text;
using UnityEngine.UI;

public class FrameServer : MonoBehaviour
{
    public Button Launch;
    public Button Close;
    public Button Clear;
    // Use this for initialization
    void Start()
    {
        SocketManager.Instance.MiniTcpServer.Launch(8888);

        MsgDispatcher.Instance.Regist(this, MsgID.HeartPack, (data) =>
        {
            SocketManager.Instance.MiniTcpServer.Send(MsgID.HeartPack, data);
        });

        MsgDispatcher.Instance.Regist(this, MsgID.Test, (data) =>
        {
            SocketManager.Instance.MiniTcpServer.Send(MsgID.Test, data);
        });
        Launch.onClick.AddListener(() =>
        {
            SocketManager.Instance.MiniTcpServer.Launch(8888);
        });
        Close.onClick.AddListener(() =>
        {
            SocketManager.Instance.MiniTcpServer.Close();
        });
        Clear.onClick.AddListener(() =>
        {
            SocketManager.Instance.MiniTcpServer.Clear();
        });
    }

}
