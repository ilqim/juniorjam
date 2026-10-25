using System.Collections.Generic;
using UnityEngine;

// Animasyonda sprite her değiştiğinde PolygonCollider2D'yi o sprite'ın şekline göre yeniler.
// Yaprak gibi animasyonlu objelere PolygonCollider2D ile birlikte ekle.
[RequireComponent(typeof(SpriteRenderer), typeof(PolygonCollider2D))]
public class SpriteColliderSync : MonoBehaviour
{
    SpriteRenderer sr;
    PolygonCollider2D poly;
    Sprite lastSprite;
    readonly List<Vector2> points = new List<Vector2>();

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        poly = GetComponent<PolygonCollider2D>();
    }

    // Animator Update'te sprite'ı değiştirir, LateUpdate'te biz yakalarız
    void LateUpdate()
    {
        if (sr.sprite == lastSprite) return;
        lastSprite = sr.sprite;
        if (lastSprite == null) return;

        int shapeCount = lastSprite.GetPhysicsShapeCount();
        poly.pathCount = shapeCount;

        for (int i = 0; i < shapeCount; i++)
        {
            points.Clear();
            lastSprite.GetPhysicsShape(i, points);
            poly.SetPath(i, points);
        }
    }
}