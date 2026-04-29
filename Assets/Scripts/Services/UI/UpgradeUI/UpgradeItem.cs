using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using VContainer;
using Excel2Unity;
using System.Collections;
public class UpgradeItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private IItembase _item;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image icon;
    [SerializeField] private RectTransform starRoot;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private GameObject boarder;
    private GameObject parentUI;
    public void OnPointerClick(PointerEventData eventData)
    {
        // YogurtGameBoard.Instance.UpgradeItem(_item.Data.GetType(), _item.Data.ID);
        parentUI.GetComponent<UpgradeUI>().ShowDetailUI(_item);
        RefreshDisplay();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        boarder.SetActive(true);
        CursorManager.Instance.SetCursor(CursorData.CursorType.Pointer);
        StartCoroutine(ShowBoarder());
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorData.CursorType.Default);
        boarder.SetActive(false);
    }
    private IEnumerator ShowBoarder()
    {
        Image boarderImage = boarder.GetComponent<Image>();
        float duration = 0.2f;
        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.95f;
        Vector3 endScale = Vector3.one;
        Color startColor = boarderImage.color;
        startColor.a = 0.5f;
        Color endColor = boarderImage.color;
        endColor.a = 1f;

        boarder.transform.localScale = startScale;
        boarderImage.color = startColor;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            boarder.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            boarderImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        boarder.transform.localScale = endScale;
        boarderImage.color = endColor;
    }
    public void SetUp<T, T2>(T2 item, GameObject parentUI) 
        where T : TableDataBase
        where T2 : Itembase<T>
    {
        this.parentUI = parentUI;
        boarder.SetActive(false);
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