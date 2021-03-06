﻿using System;
using System.Collections.Generic;
using UnityEngine;
namespace MiniFramework
{
    public partial class DebuggerComponent
    {
        private class ConsoleWindow : IDebuggerWindow
        {
            private int logCount = 0;
            private int warningCount = 0;
            private int errorCount = 0;
            private string dateTimeFormat = "[HH:mm:ss.fff]";
            private Queue<LogNode> logNodes = new Queue<LogNode>();
            private LogNode selectedNode = null;

            private bool lockScroll = true;
            private bool logFilter = true;
            private bool warningFilter = true;
            private bool errorFilter = true;

            private int logHeight = 30;
            private Color32 logColor = Color.white;
            private Color32 warningColor = Color.yellow;
            private Color32 errorColor = Color.red;
            private Vector2 logScrollPosition = Vector2.zero;
            private Vector2 stackTraceScrollPosition = Vector2.zero;
            List<LogNode> tempLogNodes = new List<LogNode>();
            private static readonly object locker = new object();
            public void Initialize(params object[] args)
            {
                Application.logMessageReceivedThreaded += OnLogMessageReceived;
            }
            public void OnUpdate(float time, float realTime) { }
            public void OnDraw()
            {
                if (Event.current.type == EventType.Layout)
                {
                    tempLogNodes = new List<LogNode>(ToArray(logNodes));
                    RefreshCount();
                }
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Clear All", GUILayout.Height(30f), GUILayout.Width(100f)))
                    {
                        Clear();
                    }
                    lockScroll = GUILayout.Toggle(lockScroll, "Lock Scroll", GUILayout.Height(30f));
                    GUILayout.FlexibleSpace();
                    logFilter = GUILayout.Toggle(logFilter, "Info (" + logCount + ")", GUILayout.Height(30f));
                    if (!logFilter)
                    {

                    }
                    warningFilter = GUILayout.Toggle(warningFilter, "Warning (" + warningCount + ")", GUILayout.Height(30f));
                    errorFilter = GUILayout.Toggle(errorFilter, "Error (" + errorCount + ")", GUILayout.Height(30f));
                }
                GUILayout.EndHorizontal();
                if (!logFilter || !warningFilter || !errorFilter)
                {
                    List<LogNode> tempFilterLogs = tempLogNodes;
                    for (int i = tempFilterLogs.Count - 1; i >= 0; i--)
                    {
                        switch (tempFilterLogs[i].LogType)
                        {
                            case LogType.Log:
                                if (!logFilter)
                                {
                                    tempFilterLogs.Remove(tempFilterLogs[i]);
                                }
                                break;
                            case LogType.Warning:
                                if (!warningFilter)
                                {
                                    tempFilterLogs.Remove(tempFilterLogs[i]);
                                }
                                break;
                            case LogType.Error:
                                if (!errorFilter)
                                {
                                    tempFilterLogs.Remove(tempFilterLogs[i]);
                                }
                                break;
                        }
                    }
                    tempLogNodes = tempFilterLogs;
                }
                GUILayout.BeginVertical("box");
                {
                    if (lockScroll)
                    {
                        logScrollPosition.y = tempLogNodes.Count * logHeight;
                    }
                    logScrollPosition = GUILayout.BeginScrollView(logScrollPosition);
                    {
                        int maxLine = 50;

                        if (maxLine > tempLogNodes.Count)
                        {
                            maxLine = tempLogNodes.Count;
                        }

                        int logIndex = (int)logScrollPosition.y / logHeight;

                        logIndex = Mathf.Clamp(logIndex, 0, tempLogNodes.Count - maxLine);

                        for (int i = 0; i < logIndex; i++)
                        {
                            GUILayout.Space(logHeight);
                        }
                        for (int i = logIndex; i < logIndex + maxLine; i++)
                        {
                            LogNode logNode = tempLogNodes[i];
                            bool isSelected = GUILayout.Toggle(selectedNode == logNode, GetLogString(logNode), GUILayout.Height(logHeight));
                            if (isSelected && selectedNode != logNode)
                            {
                                selectedNode = logNode;
                                lockScroll = false;
                                stackTraceScrollPosition = Vector2.zero;
                            }
                        }
                        for (int i = logIndex + maxLine; i < tempLogNodes.Count; i++)
                        {
                            GUILayout.Space(logHeight);
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                if (selectedNode != null)
                {
                    GUILayout.BeginVertical("box", GUILayout.Height(100));
                    stackTraceScrollPosition = GUILayout.BeginScrollView(stackTraceScrollPosition);
                    GUILayout.Label(selectedNode.LogMsg + "\n" + selectedNode.StackTrace);
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }
            }
            public void Close()
            {
                Application.logMessageReceivedThreaded -= OnLogMessageReceived;
                Clear();
            }
            private LogNode[] ToArray(Queue<LogNode> logNodes)
            {
                lock (locker)
                {
                    return logNodes.ToArray();
                }
            }
            private void Enqueue(LogNode log)
            {
                lock (locker)
                {
                    logNodes.Enqueue(log);
                }
            }
            private void OnLogMessageReceived(string logMsg, string stackTrace, LogType logtype)
            {
                if (logtype == LogType.Assert || logtype == LogType.Exception)
                {
                    logtype = LogType.Error;
                }
                LogNode log = new LogNode().Fill(logtype, logMsg, stackTrace);
                Enqueue(log);
            }
            private string GetLogString(LogNode logNode)
            {
                Color32 color = GetLogStringColor(logNode.LogType);
                string hexColor = ColorUtility.ToHtmlStringRGB(color);
                return "<color=#" + hexColor + ">" + logNode.LogTime.ToString(dateTimeFormat) + logNode.LogMsg + "</color>";
            }
            private Color32 GetLogStringColor(LogType logType)
            {
                Color32 color = Color.white;
                switch (logType)
                {
                    case LogType.Log: color = logColor; break;
                    case LogType.Warning: color = warningColor; break;
                    case LogType.Error: color = errorColor; break;
                }
                return color;
            }
            private void RefreshCount()
            {
                logCount = 0;
                warningCount = 0;
                errorCount = 0;
                foreach (LogNode logNode in tempLogNodes)
                {
                    switch (logNode.LogType)
                    {
                        case LogType.Log: logCount++; break;
                        case LogType.Warning: warningCount++; break;
                        case LogType.Error: errorCount++; break;
                    }
                }
            }
            private void Clear()
            {
                lock (locker)
                {
                    logNodes.Clear();
                    selectedNode = null;
                }
            }
        }
        /// <summary>
        /// 日志类型
        /// </summary>
        private class LogNode
        {
            public DateTime LogTime;
            public LogType LogType;
            public string LogMsg;
            public string StackTrace;
            public LogNode Fill(LogType logType, string logMsg, string stackTrace)
            {
                LogTime = DateTime.Now;
                LogType = logType;
                LogMsg = logMsg;
                StackTrace = stackTrace;
                return this;
            }
        }
    }
}