using System;
using UnityEngine;
using UnityEngine.UI;

public class StarController : MonoBehaviour
{
    [SerializeField] private Sprite empStar;
    [SerializeField] private Sprite Star;
    [SerializeField] private Material mat;
    [SerializeField] private Material defaultMat;
    private Image _image;
    void OnEnable()
    {
        _image = GetComponent<Image>();
    }
    public void SetStar()
    {
        _image.sprite = Star;
        _image.material = mat;
    }
    public void Setempty()
    {
        _image.sprite = empStar;
        _image.material = defaultMat;
    }
}