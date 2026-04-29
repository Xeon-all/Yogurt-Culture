using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DetailUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI LvTxt;
    [SerializeField] private GameObject arrow;

    [Header("入场动画配置")]
    [SerializeField] private float arrowDuration = 0.3f;
    [SerializeField] private float panelDuration = 0.3f;
    [SerializeField] private Ease positionEase = Ease.OutQuad;
    [SerializeField] private float xOffset = -10f;

    private Sequence _enterSequence;

    public void Show(IItembase item)
    {
        SetContent(item);
        PlayAnimation();
    }
    private void SetContent(IItembase item)
    {
        if(item is ToppingItem i)
        {    
            icon.sprite = Resources.Load<Sprite>(
                YogurtGameBoard.TOPPING_SPRITE + 
                i.Data.ItemIcon
            );
            nameTxt.text = i.Data.Name;
            LvTxt.text = "Lv." + item.CurLv.ToString();
        }
    }
    private void PlayAnimation()
    {
        // 清理之前的动画
        _enterSequence?.Kill();

        gameObject.SetActive(false);
        arrow.SetActive(true);

        // 初始化状态
        var arrowImg = arrow.GetComponent<Image>();
        arrowImg.color = new Color(arrowImg.color.r, arrowImg.color.g, arrowImg.color.b, 0f);
        arrowImg.fillAmount = 0f;

        // 记录原位，计算起点位置
        Vector3 arrowOriginalPos = arrow.transform.localPosition;
        Vector3 arrowStartPos = arrowOriginalPos + new Vector3(xOffset, 0f, 0f);
        arrow.transform.localPosition = arrowStartPos;

        Vector3 panelOriginalPos = transform.localPosition;
        Vector3 panelStartPos = panelOriginalPos + new Vector3(xOffset, 0f, 0f);
        transform.localPosition = panelStartPos;

        // 构建动画序列
        _enterSequence = DOTween.Sequence();
        
        // 阶段1：arrow动画（从偏移位置回到原位）
        _enterSequence.Append(arrowImg.DOFade(1f, arrowDuration).SetEase(positionEase));
        _enterSequence.Join(arrowImg.DOFillAmount(1f, arrowDuration).SetEase(positionEase));
        _enterSequence.Join(arrow.transform.DOLocalMoveX(arrowOriginalPos.x, arrowDuration).SetEase(positionEase));

        // 阶段1完成后：激活panel，播放阶段2动画
        _enterSequence.OnComplete(() =>
        {
            gameObject.SetActive(true);

            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, panelDuration).SetEase(positionEase);
            }

            transform.DOLocalMoveX(panelOriginalPos.x, panelDuration).SetEase(positionEase);
        });
    }
}