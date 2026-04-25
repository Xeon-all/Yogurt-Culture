using System;
using UnityEngine;
using Excel2Unity;
using System.Collections.Generic;

[Serializable]

public class YogurtData : TableDataBase
{
	public string Name;
	public int ExFlavor;
	public int Capability;
	public int Price;
	public bool InitActive;
	public string Tags;
	public int MaxLv;
	public string DraggerName;
	public string DraggerSprite;
	public string ItemIcon;
	public string GrooveName;
}
