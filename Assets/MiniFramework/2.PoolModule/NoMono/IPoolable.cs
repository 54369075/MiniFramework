﻿namespace MiniFramework
{
    public interface IPoolable
    {
        void OnAllocated();
        void OnRecycled();
        bool IsRecycled { get; set; }
    }
}