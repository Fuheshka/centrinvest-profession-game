using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // A thin UI ribbon with an optional quadratic bend. A zero bend is a precise straight line.
    public sealed class CurvedImage : Image
    {
        const int Segments = 28;
        const float Thickness = 7f;
        float bend;

        public void SetBend(float value)
        {
            if (Mathf.Approximately(bend, value)) return;
            bend = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = GetPixelAdjustedRect();
            var start = new Vector2(rect.xMin, rect.center.y);
            var end = new Vector2(rect.xMax, rect.center.y);
            var control = new Vector2(rect.center.x, rect.center.y + bend * 2f);
            var color32 = color;

            for (int i = 0; i < Segments; i++)
            {
                float t0 = i / (float)Segments;
                float t1 = (i + 1) / (float)Segments;
                var p0 = Bezier(start, control, end, t0);
                var p1 = Bezier(start, control, end, t1);
                var tangent = (p1 - p0).normalized;
                var normal = new Vector2(-tangent.y, tangent.x) * (Thickness * 0.5f);
                int index = vh.currentVertCount;
                AddVert(vh, p0 - normal, color32);
                AddVert(vh, p0 + normal, color32);
                AddVert(vh, p1 + normal, color32);
                AddVert(vh, p1 - normal, color32);
                vh.AddTriangle(index, index + 1, index + 2);
                vh.AddTriangle(index, index + 2, index + 3);
            }
        }

        static Vector2 Bezier(Vector2 a, Vector2 c, Vector2 b, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * c + t * t * b;
        }

        static void AddVert(VertexHelper vh, Vector2 position, Color32 tint)
        {
            var vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = tint;
            vh.AddVert(vertex);
        }
    }
}
