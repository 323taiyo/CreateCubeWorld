using UnityEngine;
using UnityEditor;

public class CCW : EditorWindow
{
    Material material;
    Texture texture;
    Vector3 cubeSize = new Vector3(0.5f, 0.5f, 0.5f);
    GameObject parentObject;
    int textureRow = 8;
    int selectTextureNum = 0;
    int[] shortcutTextureNum = new int[8];
    bool isPlay = false;

    [MenuItem("CCW/OpenWindow")]
    public static void Open()
    {
        GetWindow<CCW>();
    }

    private void OnEnable()
    {
        material = (Material)Resources.Load("CubeMaterial");
        texture = material.mainTexture;
    }

    private void OnGUI()
    {
        minSize = maxSize = new Vector2(256, 512);
        //トグルとかボタンとかフィールド
        GUILayout.BeginArea(new Rect(0, 0, 256, 128));

        if (!isPlay)
        {
            if (GUILayout.Button("Play"))
            {
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui += CubeController;
#else
                SceneView.onSceneGUIDelegate += CubeController;
#endif
                Tools.hidden = true;
                isPlay = true;
            }
        }
        else
        {
            if (GUILayout.Button("Pause"))
            {
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui -= CubeController;
#else
                SceneView.onSceneGUIDelegate -= CubeController;
#endif
                Tools.hidden = false;
                isPlay = false;
            }
        }

        cubeSize = EditorGUILayout.Vector3Field("CubeSize", cubeSize);
        textureRow = EditorGUILayout.IntField("TextureRowNum", textureRow);
        parentObject = EditorGUILayout.ObjectField("ParentObject", parentObject, typeof(GameObject), true) as GameObject;

        if (GUILayout.Button("CombineMesh"))
        {
            if (parentObject != null)
            {
                CombineMesh();
            }
        }
        GUILayout.EndArea();

        //ショートカット設定
        GUILayout.BeginArea(new Rect(0, 192, 256, 128));
        for (int i = 0; i < 8; i++)
        {
            float tilePerc = 1.0f / (float)textureRow;
            int tileX = shortcutTextureNum[i] % textureRow;
            int tileY = shortcutTextureNum[i] / textureRow;
            float umin = tilePerc * tileX;
            float vmin = tilePerc * tileY;
            GUI.DrawTextureWithTexCoords(new Rect(32 * i, 0, 32, 32), texture, new Rect(umin, vmin, tilePerc, tilePerc));
            if (GUI.Button(new Rect(32 * i, 32, 32, 16), "F" + (i + 1).ToString()))
            {
                shortcutTextureNum[i] = selectTextureNum;
            }
            //F1~8が押された時の処理
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == (KeyCode)(i + 282))
                {
                    selectTextureNum = shortcutTextureNum[i];
                    Repaint();
                }
            }
            if (selectTextureNum == shortcutTextureNum[i])
            {
                EditorGUI.DrawRect(new Rect(32 * i, 0, 32, 3), Color.cyan);
                EditorGUI.DrawRect(new Rect(32 * i, 0, 3, 32), Color.cyan);
                EditorGUI.DrawRect(new Rect(32 * i, 29, 32, 3), Color.cyan);
                EditorGUI.DrawRect(new Rect(32 * i + 29, 0, 3, 32), Color.cyan);
            }
        }
        GUILayout.EndArea();

        //テクスチャを選択するボタン
        GUILayout.BeginArea(new Rect(0, 256, 256, 256));
        GUI.DrawTexture(new Rect(0, 0, 256, 256), texture);
        int size = 256 / textureRow;
        for (int i = textureRow; i > 0; i--)
        {
            for (int j = 0; j < textureRow; j++)
            {
                if (selectTextureNum != (i - 1) * textureRow + j)
                {
                    if (GUI.Button(new Rect(j * size, (textureRow - i) * size, size, size), " ", GUIStyle.none))
                    {
                        selectTextureNum = (i - 1) * textureRow + j;
                    }
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(j * size, (textureRow - i) * size, size, 3), Color.cyan);
                    EditorGUI.DrawRect(new Rect(j * size, (textureRow - i) * size, 3, size), Color.cyan);
                    EditorGUI.DrawRect(new Rect(j * size, (textureRow - i + 1) * size - 3, size, 3), Color.cyan);
                    EditorGUI.DrawRect(new Rect((j + 1) * size - 3, (textureRow - i) * size, 3, size), Color.cyan);
                }
            }
        }
        GUILayout.EndArea();
    }

    private void CubeController(SceneView sceneView)
    {
        Vector2 mousePosition = new Vector2(Event.current.mousePosition.x * 2, Screen.height - Event.current.mousePosition.y * 2 - 80.0f);
        Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        Vector3 pos;

        //５ｍ内にオブジェクトがある場合はオブジェクトの中心から法線の向きに０．５ｍ離れた座標
        if (Physics.Raycast(ray, out hit, 5.0f))
        {
            pos = hit.transform.position + hit.normal * 0.5f;
        }
        //オブジェクトがない場合は５ｍ先の丸めた座標
        else
        {
            pos = sceneView.camera.transform.position + ray.direction * 5.0f;
            pos.x = Mathf.Round(pos.x * 2) / 2;
            pos.y = Mathf.Round(pos.y * 2) / 2;
            pos.z = Mathf.Round(pos.z * 2) / 2;
        }

        //キューブ設置場所の目安
        Handles.color = Color.cyan;
        Handles.DrawWireCube(pos, cubeSize);

        //選択中のショートカット表示
        GUILayout.BeginArea(new Rect(0, 0, 256, 32));
        for (int i = 0; i < 8; i++)
        {
            float tilePerc = 1.0f / (float)textureRow;
            int tileX = shortcutTextureNum[i] % textureRow;
            int tileY = shortcutTextureNum[i] / textureRow;
            float umin = tilePerc * tileX;
            float vmin = tilePerc * tileY;
            GUI.DrawTextureWithTexCoords(new Rect(32 * i, 0, 32, 32), texture, new Rect(umin, vmin, tilePerc, tilePerc));
            //F1~8が押されたときの処理
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == (KeyCode)(i + 282))
                {
                    selectTextureNum = shortcutTextureNum[i];
                    Repaint();
                }
            }
            if (selectTextureNum == shortcutTextureNum[i])
            {
                EditorGUI.DrawRect(new Rect(32 * i, 0, 32, 3), Color.cyan);
                EditorGUI.DrawRect(new Rect(32 * i, 0, 3, 32), Color.cyan);
                EditorGUI.DrawRect(new Rect(32 * i, 29, 32, 3), Color.cyan);
                EditorGUI.DrawRect(new Rect(32 * i + 29, 0, 3, 32), Color.cyan);
            }
        }
        GUILayout.EndArea();

        //左クリックした場合続行
        if (Event.current.type != EventType.MouseDown || Event.current.button != 0)
        {
            return;
        }

        //キューブを生成し、大きさ・座標・マテリアル・メッシュを指定
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.localScale = cubeSize;
        obj.transform.position = pos;
        obj.GetComponent<MeshRenderer>().material = material;
        Mesh mesh = Instantiate(obj.GetComponent<MeshFilter>().sharedMesh);
        mesh.uv = GetMeshUVs(selectTextureNum);
        obj.GetComponent<MeshFilter>().mesh = mesh;

        //親が指定されていない場合は生成
        if (parentObject == null)
        {
            GameObject parentObject = new GameObject("ParentObject");
            this.parentObject = parentObject;
        }
        obj.transform.parent = parentObject.transform;

        //Undo有効化
        Undo.RegisterCreatedObjectUndo(obj, "Create Cube");
    }


    private Vector2[] GetMeshUVs(int num)
    {
        float tilePerc = 1.0f / (float)textureRow;
        int tileX = num % textureRow;
        int tileY = num / textureRow;

        float umin = tilePerc * tileX;
        float umax = tilePerc * (tileX + 1);
        float vmin = tilePerc * tileY;
        float vmax = tilePerc * (tileY + 1);

        Vector2[] cubeUVs = new Vector2[24];

        //-X
        cubeUVs[2] = new Vector2(umax, vmax);
        cubeUVs[3] = new Vector2(umin, vmax);
        cubeUVs[0] = new Vector2(umax, vmin);
        cubeUVs[1] = new Vector2(umin, vmin);

        //+Y
        cubeUVs[4] = new Vector2(umin, vmin);
        cubeUVs[5] = new Vector2(umax, vmin);
        cubeUVs[8] = new Vector2(umin, vmax);
        cubeUVs[9] = new Vector2(umax, vmax);

        //-Z
        cubeUVs[23] = new Vector2(umax, vmin);
        cubeUVs[21] = new Vector2(umin, vmax);
        cubeUVs[20] = new Vector2(umin, vmin);
        cubeUVs[22] = new Vector2(umax, vmax);

        //+Z
        cubeUVs[19] = new Vector2(umax, vmin);
        cubeUVs[17] = new Vector2(umin, vmax);
        cubeUVs[16] = new Vector2(umin, vmin);
        cubeUVs[18] = new Vector2(umax, vmax);

        //-Y
        cubeUVs[15] = new Vector2(umax, vmin);
        cubeUVs[13] = new Vector2(umin, vmax);
        cubeUVs[12] = new Vector2(umin, vmin);
        cubeUVs[14] = new Vector2(umax, vmax);

        //+X
        cubeUVs[6] = new Vector2(umin, vmin);
        cubeUVs[7] = new Vector2(umax, vmin);
        cubeUVs[10] = new Vector2(umin, vmax);
        cubeUVs[11] = new Vector2(umax, vmax);

        return cubeUVs;
    }

    private void CombineMesh()
    {
        //親オブジェクト配下のキューブのメッシュを結合用配列に格納
        MeshFilter[] meshFilters = parentObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        //結合先のオブジェクト生成
        GameObject cubes = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubes.GetComponent<MeshRenderer>().material = material;

        //メッシュ生成
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine, true);
        cubes.transform.GetComponent<MeshFilter>().mesh = mesh;

        //ボックスコライダーを削除してメッシュコライダーに変更     
        DestroyImmediate(cubes.GetComponent<BoxCollider>());
        MeshCollider meshCollider = cubes.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        //結合元のキューブを非表示
        parentObject.transform.gameObject.SetActive(false);
        parentObject = null;
    }
}
