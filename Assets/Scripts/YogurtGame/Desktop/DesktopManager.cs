using UnityEngine;

public class DesktopManager : Singleton<DesktopManager>
{
    private const string PREFABFILE = "Prefabs/GameFunc/";

    public GameObject SpawnToppingDragger(ToppingItem item)
    {
        if (item?.Data == null) return null;

        var draggerPrefab = Resources.Load<GameObject>(PREFABFILE + item.Data.DraggerName);
        if (draggerPrefab == null) return null;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(mouseScreen);

        var instance = Instantiate(draggerPrefab, spawnPos, Quaternion.identity);
        instance.GetComponent<DraggerTopping>().SetData(item);
        return instance;
    }
}
