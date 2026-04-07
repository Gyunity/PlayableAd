using UnityEngine;

[ExecuteInEditMode]
public class TextureTilingMPB : MonoBehaviour
{
    public float tilePerUnit = 1f;

    void Start() => Apply();
    void OnValidate() => Apply();

    void Apply()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        var block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);

        Vector3 s = transform.localScale;
        block.SetVector("_MainTex_ST", new Vector4(
            s.x * tilePerUnit,  // Tiling X
            s.y * tilePerUnit,  // Tiling Y
            0, 0                // Offset
        ));

        rend.SetPropertyBlock(block);
    }
}