using System;
using UnityEngine;
using VContainer.Unity;

public class StartGameTor : ITutorialHandler
{
    public bool CheckCondition()
    {
        return true;
    }
    public void Execute()
    {
        Debug.Log("准备教学");
    }
}