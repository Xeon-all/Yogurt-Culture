using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using VContainer;
using Excel2Unity;
public class UpgradeItem : MonoBehaviour, IPointerClickHandler
{
    private IItembase _item;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image icon;
    [SerializeField] private RectTransform starRoot;
    [SerializeField] private GameObject starPrefab;
    public void OnPointerClick(PointerEventData eventData)
    {
        YogurtGameBoard.Instance.UpgradeItem(_item.Data.GetType(), _item.Data.ID);
        RefreshDisplay();
    }
    public void SetUp<T, T2>(T2 item) 
        where T : TableDataBase
        where T2 : Itembase<T>
    {
        _item = item;
        nameText.text = item.Data.ID;
        if(item is ToppingItem i)
        {
            var d = item.Data as ToppingData;
            nameText.text = d.Name;
            icon.sprite = Resources.Load<Sprite>(
                YogurtGameBoard.TOPPING_SPRITE + 
                d.ItemIcon
            );
            SetLvStar(d.MaxLv, i.CurLv);
        }
        else if(item is YogurtItem it)
        {
            var d = item.Data as YogurtData;
            nameText.text = d.Name;
            icon.sprite = Resources.Load<Sprite>(
                YogurtGameBoard.YOGURT_SPRITE + 
                d.ItemIcon
            );
            SetLvStar(d.MaxLv, it.CurLv);
        }
    }
    public void RefreshDisplay()
    {
        SetLvStar(_item.MaxLv, _item.CurLv);
    }
    private void SetLvStar(int max, int cur)
    {
        foreach(Transform c in starRoot)
            Destroy(c.gameObject);
        for(int i = 0; i < max; i++)
        {
            GameObject obj = Instantiate(starPrefab, starRoot);
            if(i < cur)
                obj.GetComponent<StarController>().SetStar();
            else
                obj.GetComponent<StarController>().Setempty();
            AspectRatioFitter fitter = obj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = 1f;
            LayoutRebuilder.ForceRebuildLayoutImmediate(starRoot);
        }
    }
}