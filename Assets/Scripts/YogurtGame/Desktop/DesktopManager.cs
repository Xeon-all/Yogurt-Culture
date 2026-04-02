using System;
using UnityEngine;

public class DesktopManager : Singleton<DesktopManager>
{
    private const string PREFABFILE = "Prefabs/GameFunc/";
    public GameObject SpawnToppingDragger(ToppingData topping)
    {
        // Debug.Log("Searching " + PREFABFILE + topping.DraggerName);
        if(topping == null) return null;
        var draggerPrefab = Resources.Load<GameObject>(PREFABFILE + topping.DraggerName);

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(mouseScreen);

        var instance = Instantiate(draggerPrefab, spawnPos, Quaternion.identity);
        instance.GetComponent<DraggerTopping>().SetData(topping);
        return instance;
    }
}