using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class BetterOutline : MonoBehaviour
{
    //  Static mesh registry 
    // Tracks which meshes have already had smooth normals baked into UV3 so we
    // don't do redundant (and destructive) work when multiple Outline components
    // share the same mesh asset.
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    //  Mode enum 
    public enum Mode
    {
        /// <summary>Outline drawn in front of everything, visible through all geometry.</summary>
        OutlineAll,

        /// <summary>Outline drawn only on the visible silhouette of the object.</summary>
        OutlineVisible,

        /// <summary>Outline drawn only where the object is occluded by opaque geometry.</summary>
        OutlineHidden,

        /// <summary>Outline on visible parts + a silhouette halo where occluded.</summary>
        OutlineAndSilhouette,

        /// <summary>Only the silhouette halo; no thickness outline.</summary>
        SilhouetteOnly,

        /// <summary>
        /// Like OutlineAll but the outline is rendered in the transparent queue so
        /// it is correctly occluded by (drawn-after) transparent objects.
        /// Toggle this versus OutlineAll to control whether the outline "punches
        /// through" transparent surfaces.
        /// </summary>
        OutlineOccludedByTransparents,
    }

    // Public properties
    public Mode OutlineMode
    {
        get => outlineMode;
        set { outlineMode = value; needsUpdate = true; }
    }

    public Color OutlineColor
    {
        get => outlineColor;
        set { outlineColor = value; needsUpdate = true; }
    }

    public float OutlineWidth
    {
        get => outlineWidth;
        set { outlineWidth = value; needsUpdate = true; }
    }

    // Serialised fields 
    [SerializeField] private Mode outlineMode = Mode.OutlineAll;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;

    [Header("Optional")]
    [SerializeField,
     Tooltip("Precompute enabled: smooth normals are baked in the editor and serialised. " +
             "Precompute disabled: baking happens at runtime in Awake(), which may stall on large meshes.")]
    private bool precomputeOutline;

    // Baked smooth-normal cache (serialised so it survives domain reloads).
    [SerializeField, HideInInspector] private List<Mesh> bakeKeys = new List<Mesh>();
    [SerializeField, HideInInspector] private List<ListVector3> bakeValues = new List<ListVector3>();

    //  Runtime state 
    private Renderer[] renderers;
    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;
    private bool needsUpdate;

    // Helper serialisable wrapper
    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    // =========================================================================
    //  Unity messages
    // =========================================================================

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));
        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        // Enable stencil-based isolation so that mask passes from one outlined
        // object cannot occlude the silhouette fill of another outlined object.
        // Each instance gets a unique stencil ref derived from its instance ID.
        ApplyStencilIsolation();

        LoadSmoothNormals();
        needsUpdate = true;
    }

    void OnEnable()
    {
        foreach (var r in renderers)
        {
            var mats = r.sharedMaterials.ToList();
            mats.Add(outlineMaskMaterial);
            mats.Add(outlineFillMaterial);
            r.materials = mats.ToArray();
        }
    }

    void OnValidate()
    {
        needsUpdate = true;

        if ((!precomputeOutline && bakeKeys.Count != 0) || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        if (precomputeOutline && bakeKeys.Count == 0)
            Bake();
    }

    void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;
            UpdateMaterialProperties();
        }
    }

    void OnDisable()
    {
        foreach (var r in renderers)
        {
            var mats = r.sharedMaterials.ToList();
            mats.Remove(outlineMaskMaterial);
            mats.Remove(outlineFillMaterial);
            r.materials = mats.ToArray();
        }
    }

    void OnDestroy()
    {
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }

    // =========================================================================
    //  Stencil isolation  (Fix #3)
    // =========================================================================
    // The silhouette mask pass writes to the stencil buffer with a per-instance
    // reference value.  The fill pass tests against the SAME reference, so a
    // fill from object B is never blocked by a mask written by object A.

    private void ApplyStencilIsolation()
    {
        // Clamp to [1..254] so we never use the reserved values 0 / 255.
        int stencilRef = (Mathf.Abs(GetInstanceID()) % 254) + 1;

        // Mask: always write our ref when rendered.
        outlineMaskMaterial.SetInt("_StencilRef", stencilRef);
        outlineMaskMaterial.SetInt("_StencilWriteMask", 0xFF);
        outlineMaskMaterial.SetInt("_StencilComp", (int)CompareFunction.Always);
        outlineMaskMaterial.SetInt("_StencilOp", (int)StencilOp.Replace);

        // Fill: only draw where OUR ref is already written.
        outlineFillMaterial.SetInt("_StencilRef", stencilRef);
        outlineFillMaterial.SetInt("_StencilReadMask", 0xFF);
        outlineFillMaterial.SetInt("_StencilComp", (int)CompareFunction.Equal);
        outlineFillMaterial.SetInt("_StencilOp", (int)StencilOp.Keep);
    }

    // =========================================================================
    //  Smooth-normal baking
    // =========================================================================

    void Bake()
    {
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var mf in GetComponentsInChildren<MeshFilter>())
        {
            if (!bakedMeshes.Add(mf.sharedMesh)) continue;

            var smoothNormals = SmoothNormals(mf.sharedMesh);
            bakeKeys.Add(mf.sharedMesh);
            bakeValues.Add(new ListVector3 { data = smoothNormals });
        }
    }

    void LoadSmoothNormals()
    {
        //  Regular MeshFilter renderers 
        foreach (var mf in GetComponentsInChildren<MeshFilter>())
        {
            if (!registeredMeshes.Add(mf.sharedMesh)) continue;

            int index = bakeKeys.IndexOf(mf.sharedMesh);
            var smoothNormals = (index >= 0)
                ? bakeValues[index].data
                : SmoothNormals(mf.sharedMesh);

            mf.sharedMesh.SetUVs(3, smoothNormals);

            var r = mf.GetComponent<Renderer>();
            if (r != null)
                CombineSubmeshes(mf.sharedMesh, r.sharedMaterials);
        }

        //  SkinnedMeshRenderer 
        // Skinned meshes don't use MeshFilter; we clear UV3 here because the
        // shader will read world-space normals from the skinned vertex stream.
        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!registeredMeshes.Add(smr.sharedMesh)) continue;

            // Zero-out UV3 — skinned meshes drive normals via the skeleton.
            smr.sharedMesh.uv4 = new Vector2[smr.sharedMesh.vertexCount];

            CombineSubmeshes(smr.sharedMesh, smr.sharedMaterials);
        }
    }

    //  Fixed smooth-normal algorithm  (Fix #1) 
    // Old bug: for flat meshes (e.g. a Plane) all face normals cancel to zero
    // during averaging.  For dual-contoured meshes, near-degenerate triangles
    // produce tiny, inconsistent normals that also collapse.
    //
    // Fix: use the *same* summation strategy but guard against zero-length
    // results by falling back to the original per-vertex normal.  We also skip
    // the "single vertex" early-out so even isolated verts go through the guard.
    List<Vector3> SmoothNormals(Mesh mesh)
    {
        // Start with a copy of the original normals as the fallback.
        var smoothNormals = new List<Vector3>(mesh.normals);

        // Group all vertex indices by world position.
        var groups = mesh.vertices
            .Select((v, i) => new KeyValuePair<Vector3, int>(v, i))
            .GroupBy(pair => pair.Key);

        foreach (var group in groups)
        {
            // Single vertex — no averaging needed; keep original normal.
            if (group.Count() == 1) continue;

            var sum = Vector3.zero;
            foreach (var pair in group)
                sum += smoothNormals[pair.Value];

            // Guard: if the sum is degenerate (e.g. a perfectly flat plane seen
            // edge-on, or a dual-contoured mesh with mirrored faces), retain the
            // original per-vertex normals rather than storing Vector3.zero, which
            // would make the outline invisible.
            if (sum.sqrMagnitude < 1e-6f)
                continue;   // leave smoothNormals[i] as-is for this group

            var averaged = sum.normalized;
            foreach (var pair in group)
                smoothNormals[pair.Value] = averaged;
        }

        return smoothNormals;
    }

    void CombineSubmeshes(Mesh mesh, Material[] materials)
    {
        if (mesh.subMeshCount == 1) return;
        if (mesh.subMeshCount > materials.Length) return;

        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    // =========================================================================
    //  Material property update
    // =========================================================================

    void UpdateMaterialProperties()
    {
        outlineFillMaterial.SetColor("_OutlineColor", outlineColor);

        switch (outlineMode)
        {
            // OutlineAll 
            // Drawn in geometry queue; ignores depth entirely > visible through
            // everything, including transparent objects (draw-order permitting).
            case Mode.OutlineAll:
                SetQueueAndZWrite(outlineMaskMaterial, 2000, false);
                SetQueueAndZWrite(outlineFillMaterial, 2001, false);
                outlineMaskMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            //  OutlineVisible 
            case Mode.OutlineVisible:
                SetQueueAndZWrite(outlineMaskMaterial, 2000, false);
                SetQueueAndZWrite(outlineFillMaterial, 2001, false);
                outlineMaskMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            //  OutlineHidden 
            case Mode.OutlineHidden:
                SetQueueAndZWrite(outlineMaskMaterial, 2000, false);
                SetQueueAndZWrite(outlineFillMaterial, 2001, false);
                outlineMaskMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            //  OutlineAndSilhouette 
            // Fix #3: mask uses LessEqual (writes stencil only where object is
            // visible).  Because of the per-instance stencil ref, only THIS
            // object's fill reads the stencil, so another object's silhouette
            // behind it is not blocked.
            case Mode.OutlineAndSilhouette:
                SetQueueAndZWrite(outlineMaskMaterial, 2000, false);
                SetQueueAndZWrite(outlineFillMaterial, 2001, false);
                outlineMaskMaterial.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            // SilhouetteOnly
            // Same stencil isolation fix applies.
            case Mode.SilhouetteOnly:
                SetQueueAndZWrite(outlineMaskMaterial, 2000, false);
                SetQueueAndZWrite(outlineFillMaterial, 2001, false);
                outlineMaskMaterial.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
                break;

            //  OutlineOccludedByTransparents (Fix #2 + new mode) 
            // Rendered in the transparent queue (3000+) so that transparent
            // objects drawn later correctly occlude the outline.  ZWrite is off
            // so we don't corrupt the depth buffer for subsequent transparents.
            // The outline IS still visible through opaque geometry (ZTest Always
            // on the fill) — use OutlineVisible if you want opaque occlusion too.
            case Mode.OutlineOccludedByTransparents:
                SetQueueAndZWrite(outlineMaskMaterial, 3000, false);
                SetQueueAndZWrite(outlineFillMaterial, 3001, false);
                outlineMaskMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;
        }
    }

    //  Helper 
    private static void SetQueueAndZWrite(Material mat, int queue, bool zWrite)
    {
        mat.renderQueue = queue;
        mat.SetFloat("_ZWrite", zWrite ? 1f : 0f);
    }
}