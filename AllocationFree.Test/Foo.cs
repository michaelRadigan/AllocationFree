using System;
using AllocationFree;

[AllocationFree.AllocationFree]
public class TestClass
{
    public string Name { get; set; }
    public int[] Array = new [] {0, 1, 2};
}