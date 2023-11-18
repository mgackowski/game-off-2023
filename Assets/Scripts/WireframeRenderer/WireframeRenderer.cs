/**
 * Draws a wireframe around the object's mesh using Unity's GL interface.
 * Can remove diagonals to give the illusion of using quads.
 * Must run PrepareLineSegments() method first and whenever mesh geometry changes. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WireframeRenderer : MonoBehaviour
{
    [SerializeField] RenderMode renderMode = RenderMode.Quads;
    [SerializeField] bool includeDiagonals = false;
    [SerializeField] Material lineMaterial;
    [SerializeField] float width = 0.01f;

    [SerializeField] List<LineSegment> lineSegments = new List<LineSegment>();

    public enum RenderMode
    {
        Lines, Quads
    }

    [Serializable]
    struct LineSegment : IEquatable<LineSegment>
    {
        [SerializeField]
        internal Vector3 aPos, bPos, aNormal, bNormal;

        /* Two segments are equal if they share points regardless of
         * which one is a or b; normals must match */
        public bool Equals(LineSegment other)
        {
            return
                ((aPos == other.aPos && bPos == other.bPos) ||
                (aPos == other.bPos && bPos == other.aPos)) &&
                aNormal == other.aNormal &&
                bNormal == other.bNormal;
        }
    }

    public void OnRenderObject()
    {
        /* Prevent rendering to every small preview window in the editor */
        if (Camera.current && Camera.current.cameraType == CameraType.Preview)
        {
            return;
        }

        if (lineSegments == null || lineSegments.Count == 0)
        {
            return;
        }

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        if (renderMode == RenderMode.Lines)
        {
            GL.Begin(GL.LINES);

            foreach (LineSegment line in lineSegments)
            {
                GL.Vertex3(line.aPos.x, line.aPos.y, line.aPos.z);
                GL.Vertex3(line.bPos.x, line.bPos.y, line.bPos.z);
            }
        }
        else if (renderMode == RenderMode.Quads)
        {
            GL.Begin(GL.QUADS);

            foreach (LineSegment line in lineSegments)
            {
                Vector3 perpendicular = Vector3.Cross(
                    line.aNormal, line.bPos - line.aPos);

                Vector3 aPosL = line.aPos + perpendicular * width;
                Vector3 aPosR = line.aPos - perpendicular * width;
                Vector3 bPosL = line.bPos + perpendicular * width;
                Vector3 bPosR = line.bPos - perpendicular * width;

                GL.Vertex3(aPosL.x, aPosL.y, aPosL.z);
                GL.Vertex3(aPosR.x, aPosR.y, aPosR.z);
                GL.Vertex3(bPosR.x, bPosR.y, bPosR.z);
                GL.Vertex3(bPosL.x, bPosL.y, bPosL.z);
            }
        }
        GL.End();
        GL.PopMatrix();
    }

    public void PrepareLineSegments()
    {
        lineSegments.Clear();

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        mesh.GetVertices(vertices);
        mesh.GetTriangles(triangles, 0); //TODO: submesh 0 hardcoded
        mesh.GetNormals(normals);

        Vector3 loopBackVertex = Vector3.zero;
        Vector3 loopBackNormal = Vector3.zero;

        for (int i = 1; i < triangles.Count + 1; i++)
        {
            if (i % 3 == 1)
            {
                loopBackVertex = vertices[triangles[i - 1]];
                loopBackNormal = normals[triangles[i - 1]];
            }
            if (i % 3 == 0)
            {
                lineSegments.Add(new LineSegment
                {
                    aPos = vertices[triangles[i - 1]],
                    bPos = loopBackVertex,
                    aNormal = normals[triangles[i - 1]],
                    bNormal = loopBackNormal
                });
            }
            else
            {
                lineSegments.Add(new LineSegment
                {
                    aPos = vertices[triangles[i - 1]],
                    bPos = vertices[triangles[i]],
                    aNormal = normals[triangles[i - 1]],
                    bNormal = normals[triangles[i]]
                });
            }
        }

        if (!includeDiagonals)
        {
            FilterOutDiagonals();
        }

    }

    public void PrepareAllInScene()
    {
       foreach (WireframeRenderer wireframe in FindObjectsOfType<WireframeRenderer>())
        {
            wireframe.PrepareLineSegments();
        } 
    }

    internal void FilterOutDiagonals()
    {
        /* Remove every element that occured more than once (including original);
         * since diagonals will be where two segments share points and normals */
        List<LineSegment> diagonals = lineSegments.GroupBy(x => x)
            .Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        lineSegments.RemoveAll(x => diagonals.Any(
            y => EqualityComparer<LineSegment>.Default.Equals(x, y)));
    }

}
