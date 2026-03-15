using System;
using UnityEngine;
using Excel2Unity;
using System.Collections.Generic;

[Serializable]

public class UpgradeData : TableDataBase
{
	public string Description;
	public int Price;
	public string MethodName;
	public string Params;
}
