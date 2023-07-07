using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshBuilder {

    public enum Shading {
        Smooth,
        Flat
    }

    public static Mesh GenerateMesh (int numQuadsPerSide, float overallWidth) => GenerateMesh (numQuadsPerSide, overallWidth, Shading.Smooth);

    public static Mesh GenerateMesh (int numQuadsPerSide, float overallWidth, Shading shading) {
        Mesh m = new Mesh ();

        List<Vector3> verts = new List<Vector3> ();
        List<int> tris = new List<int> ();
        List<Vector2> uv = new List<Vector2> ();

        float quadWidth = overallWidth / numQuadsPerSide;

        Vector3 start = new Vector3 (-overallWidth / 2, 0, -overallWidth / 2);

        if (shading == Shading.Smooth) {
            int numVertsPerSide = numQuadsPerSide + 1;
            int vertIndex = 0;
            for (int i = 0; i < numVertsPerSide; i++) {
                for (int j = 0; j < numVertsPerSide; j++) {
                    verts.Add (start + Vector3.right * i * quadWidth + Vector3.forward * j * quadWidth);
                    uv.Add (new Vector2 (i / numQuadsPerSide, j / numQuadsPerSide));

                    if(i != numVertsPerSide - 1 && j != numVertsPerSide - 1) {
                        tris.Add (vertIndex);
                        tris.Add (vertIndex + 1);
                        tris.Add (vertIndex + numVertsPerSide);
                        tris.Add (vertIndex + 1);
                        tris.Add (vertIndex + numVertsPerSide + 1);
                        tris.Add (vertIndex + numVertsPerSide);
                    }

                    vertIndex++;
                }
            }
        }

        m.vertices = verts.ToArray ();
        m.triangles = tris.ToArray ();
        m.uv = uv.ToArray ();

        m.RecalculateBounds ();
        m.RecalculateNormals ();
        m.RecalculateTangents ();

        return m;
    }
}
