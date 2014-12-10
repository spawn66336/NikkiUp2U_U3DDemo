using UnityEngine;
using System.Collections;

public class MainViewCtrl : EditorControl 
{ 
    public RenderTexture mainViewTexture = null;
    public GameObject mainViewRoot = null;
    public GameObject editObjBindingTarget = null;
    public GameObject refModelBindingTarget = null;
    //网格线
    public GameObject gridMeshObj = null;
    public GameObject camObj = null;
    public Camera mainCam = null; 
    public GameObject mainLight = null;

    public bool rotateDragging = false;
    public bool moveDragging = false;
    public bool zoomDragging = false;
    public bool zoomWheelScroll = false;

    //球面相机参数 
    public float radius = 10.0f;
    public float minRadius = 2.0f;
    public float maxRadius = 100.0f;
    public Vector3 center = new Vector3();

    public bool Is2DView
    {
        get { return is2DView; }
        set { is2DView = value; }
    }

    //背景色
    public Color bkColor = Color.black;

    //是否为2D视图
    private bool is2DView = false;
    //当前MainView数据是否初始化
    private bool initFlag = false;



    public MainViewCtrl() {}

    //在第一次渲染此控件时调用
    public void Init()
    {
        if( initFlag )
        {
            return;
        }
        initFlag = true;


        if (mainViewTexture == null)
        {
            mainViewTexture = new RenderTexture(512, 512, 24);
        }

        if (mainViewRoot == null)
        {
            mainViewRoot = new GameObject();
            mainViewRoot.name = _GenMainViewRootName();
            //为此视图分配唯一的原点
            mainViewRoot.transform.position = _GenMainViewOrigin();
            if (!EditorHelper.IsDebugMode())
            {
                mainViewRoot.hideFlags = HideFlags.HideAndDontSave;
            }
            mainViewRoot.layer = MainViewCtrl.s_layer;
        }

        if (refModelBindingTarget == null)
        {
            refModelBindingTarget = new GameObject();
            refModelBindingTarget.name = "_RefModelBindingPoint";
            refModelBindingTarget.transform.parent = mainViewRoot.transform;
            refModelBindingTarget.transform.localPosition = Vector3.zero;
            if (!EditorHelper.IsDebugMode())
            {
                refModelBindingTarget.hideFlags = HideFlags.HideAndDontSave;
            }
            refModelBindingTarget.layer = MainViewCtrl.s_layer;
        }

        if (editObjBindingTarget == null)
        {
            editObjBindingTarget = new GameObject();
            editObjBindingTarget.name = "_EditObjBindingPoint";
            editObjBindingTarget.transform.parent = mainViewRoot.transform;
            editObjBindingTarget.transform.localPosition = Vector3.zero;
            if (!EditorHelper.IsDebugMode())
            {
                editObjBindingTarget.hideFlags = HideFlags.HideAndDontSave;
            }
            editObjBindingTarget.layer = MainViewCtrl.s_layer;
        }

        //非3D视图不创建栅格网格
        if (gridMeshObj == null && !Is2DView)
        {
            gridMeshObj = new GameObject();
            gridMeshObj.name = "_GridMeshObject";
            gridMeshObj.transform.parent = mainViewRoot.transform;
            gridMeshObj.transform.localPosition = Vector3.zero;
            if (!EditorHelper.IsDebugMode())
            {
                gridMeshObj.hideFlags = HideFlags.HideAndDontSave;
            }
            gridMeshObj.layer = MainViewCtrl.s_layer;

            MeshRenderer gridMeshRenderer = gridMeshObj.AddComponent<MeshRenderer>();

            gridMeshRenderer.material = new Material(Shader.Find("Diffuse"));
            MeshFilter gridMeshFilter = gridMeshObj.AddComponent<MeshFilter>();
            Mesh gridMesh = new Mesh();
            _BuildGridMesh(gridMesh);
            gridMeshFilter.mesh = gridMesh;

        }

        if (mainCam == null)
        {
            camObj = new GameObject();
            camObj.name = "_MainViewCamera";
            camObj.transform.parent = mainViewRoot.transform;

            if (!is2DView)
            {
                camObj.transform.localPosition = new Vector3(0, 0, -5);
            }
            else
            {
                camObj.transform.localPosition = new Vector3(0, 0, -10);
            }

            camObj.layer = MainViewCtrl.s_layer;
            center = Vector3.zero;
            radius = 10f;

            if (!EditorHelper.IsDebugMode())
            {
                camObj.hideFlags = HideFlags.HideAndDontSave;
            }

            //若为3D视口
            if(!is2DView)
            {//初始化相机位置
                Transform camTrans = camObj.transform;
                float angleRotateAroundUp = 135.0f;
                float angleRotateAroundRight = 45.0f;

                Vector3 localPos = (camTrans.localPosition - center).normalized * radius;

                Quaternion q0 = Quaternion.AngleAxis(angleRotateAroundUp, camTrans.up);
                camTrans.localPosition = q0 * localPos;
                camTrans.Rotate(Vector3.up, angleRotateAroundUp, Space.Self);

                Quaternion q1 = Quaternion.AngleAxis(angleRotateAroundRight, camTrans.right);
                camTrans.Rotate(Vector3.right, angleRotateAroundRight, Space.Self);
                camTrans.localPosition = q1 * camTrans.localPosition;
                camTrans.localPosition += center;
            } 
             

            Camera camera = camObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = bkColor;
                        
            
            camera.nearClipPlane = 1f;
            camera.farClipPlane = 1000f;
            camera.targetTexture = mainViewTexture;

            if (!is2DView)
            {
                camera.isOrthoGraphic = false;
                camera.aspect = 1.0f;
            }
            else
            {
                camera.isOrthoGraphic = true; 
                camera.orthographicSize = 10f;
            }
            camera.cullingMask = ~0;
            mainCam = camera;
        }

        if (mainLight == null)
        {
            //将方向光挂接在相机下
            mainLight = new GameObject();
            mainLight.name = "_MainDirLight";
            mainLight.transform.parent = camObj.transform;
            mainLight.transform.localPosition = Vector3.zero;
            mainLight.transform.localRotation = Quaternion.identity;
            if (!EditorHelper.IsDebugMode())
            {
                mainLight.hideFlags = HideFlags.HideAndDontSave;
            }
            Light light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.2f;
            mainLight.layer = MainViewCtrl.s_layer;

        }
    }

    public void Resize( Rect viewSize )
    {
        Init();
        mainViewTexture = new RenderTexture((int)viewSize.width, (int)viewSize.height, 24); 
        mainCam.targetTexture = mainViewTexture;
        mainCam.aspect = viewSize.width / viewSize.height;
    }

   
    
    public GameObject GetBindingTarget()
    {
        Init();
        return editObjBindingTarget;
    }

    public GameObject GetRefModelBindingTarget()
    {
        Init();
        return refModelBindingTarget;
    }

    public override GUIStyle GetStyle()
    {
        return SpecialEffectEditorStyle.PanelBox;
    }

    public override GUILayoutOption[] GetOptions()
    {
        return new GUILayoutOption[] {  
            GUILayout.ExpandHeight(true), 
            GUILayout.ExpandWidth(true) };
    }

    private void _BuildGridMesh(Mesh mesh)
    {
        int gridNum = 100;
        int vertNum = 2 * gridNum * 2;

        Vector3[] verts = new Vector3[vertNum];
        Vector3[] norms = new Vector3[vertNum];
        Vector2[] uvs = new Vector2[vertNum];
        int[] indices = new int[vertNum];

        float gridWidth = 10f;
        Vector3 startPoint = new Vector3(-gridWidth * (gridNum / 2), 0.0f, -gridWidth * (gridNum / 2));
        //startPoint = Vector3.zero;
        int indx = 0;


        for (int i = 0; i < gridNum; i++)
        {
            verts[indx++] = startPoint + new Vector3(0.0f, 0.0f, i * gridWidth);
            verts[indx++] = startPoint + new Vector3(gridWidth * gridNum, 0.0f, i * gridWidth);
        }

        for (int i = 0; i < gridNum; i++)
        {
            verts[indx++] = startPoint + new Vector3(i * gridWidth, 0.0f, 0.0f);
            verts[indx++] = startPoint + new Vector3(i * gridWidth, 0.0f, gridWidth * gridNum);
        }

        for (int i = 0; i < vertNum; i++)
        {
            norms[i] = Vector3.up;
            indices[i] = i;
        }

        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
    }
    //分配主视图原点
    private static Vector3 _GenMainViewOrigin()
    {
        float offset = 1000.0f;
        Vector3 origin = s_mainViewOrigin;
        s_mainViewOrigin.x += offset;
        return origin;
    }
 
    private static string _GenMainViewRootName()
    {
        string mainViewRootName =  "_MainViewRoot" + mainViewCount.ToString();
        mainViewCount++;
        return mainViewRootName;
    }


    private static int s_layer = 30;
    private static Vector3 s_mainViewOrigin = new Vector3(10000f, 10000f, 10000f);
    private static int mainViewCount = 0;
}
