using UnityEngine;
using UnityEngine.UI;

public class RadarChart : MaskableGraphic
{
    [Range(0,1)] public float[] values;        // 各轴数值 (0~1)
    public int sides = 5;
    public Color fillColor = Color.cyan;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (values == null || values.Length < 3) return;

        float angleStep = 360f / sides;
        float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;

        // 1. 填充多边形顶点
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = fillColor;
        Vector2 center = Vector2.zero;
        vertex.position = Vector2.zero;
        vh.AddVert(vertex);

        for (int i = 0; i < sides; i++)
        {
            float angle = (90f - i * angleStep) * Mathf.Deg2Rad; // 从顶部开始
            float r = radius * values[i % values.Length];
            vertex.position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
            vh.AddVert(vertex);
        }

        // 2. 构建三角形（风扇形）
        for (int i = 1; i < sides; i++)
            vh.AddTriangle(0, i, i + 1);
        vh.AddTriangle(0, sides, 1);
        // 可选：绘制边框
        // 用 AddVert + AddTriangle 或另写一个类画线段
    }
}