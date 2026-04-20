using System;
using UnityEngine;
using Excel2Unity;
using System.Collections.Generic;

[Serializable]

public class YogurtDatabase : TableDataBase
{
	public string Name;
	public int ExFlavor;
	public int Capability;
	public int Price;
	public string Tags;
}
