using UnityEngine;

public class DesktopManager : Singleton<DesktopManager>
{

    public GameObject SpawnToppingDragger(ToppingItem item)
    {
        if (item?.Data == null) return null;

        var draggerPrefab = Resources.Load<GameObject>(YogurtGameBoard.DRAGGER_PREFAB + item.Data.DraggerName);
        var draggerSprite = Resources.Load<Sprite>(YogurtGameBoard.TOPPING_SPRITE + item.Data.DraggerSprite);
        if (draggerPrefab == null) return null;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(mouseScreen);

        var instance = Instantiate(draggerPrefab, spawnPos, Quaternion.identity);
        if(draggerSprite != null)
            instance.GetComponent<SpriteRenderer>().sprite = draggerSprite;
        instance.GetComponent<DraggerTopping>().SetData(item);
        return instance;
    }
    public void ClearAllTopping()
    {
        var spawners = FindObjectsByType<ToppingSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var s in spawners)
            s.ClearContain();
    }
}
