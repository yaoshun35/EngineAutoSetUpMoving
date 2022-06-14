using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pixyz.ImportSDK;

public class TSEngineEdiorToolWindow :EditorWindow
{

    //该脚本主要功能是清理不可见对象、批量替换材质。
   
    //窗口启动
    
    [MenuItem("TSAeroEngineTool/TSEngineCADTool %t", false, 11)]

     static void OpenCADToolWindow() {

        Rect wr = new Rect(0, 0, 500, 800);
        TSEngineEdiorToolWindow window = (TSEngineEdiorToolWindow)EditorWindow.GetWindowWithRect(typeof(TSEngineEdiorToolWindow), wr, true, "TSEngineCADTool-清理、材质替换");
        window.Show();
    
    }


    //以下开始撰写窗口信息
    //航发院logo
    private Texture _texture;

    //材质替换功能所用变量：MD文件里的材质字段名称、值，以及对应的本地材质名称
    private string _MDMaterialPropertyName= "tsMaterial";
    private string _MDMaterialValueName="请输入CAD材质编号";
    private string _LoacalMatarialName="请输入要替换的本地材质名称";
    //对象池、存放符合条件的、即将替换材质的对象
    List<Transform> TS_OBJNeedToChangeMaterial = new List<Transform>();
    //当前对象及其子对象包含的所有材质的名称
    List<string> Material_ObjectContained = new List<string>();

    //统计已选对象数量，共包含MetaData组件的数量
    private int SelectedCount;
    int MetaDataObjCount = 0;
    //拥有MD的对象池
    List<Transform> TS_OBJSWithMetaData = new List<Transform>();

    //即将清理的对象池
    List<Transform> TS_OBJNeedToClear = new List<Transform>();




    public void Awake()
    {
        _texture = Resources.Load("logo") as Texture;
    }

    private void OnGUI()
    {
        //绘制置顶的logo
        GUILayout.Label(_texture);


        //关于当前选择对象的信息
        GUILayout.Label("您选择了" + Selection.count + "个对象");

        //如果用户只选择了一个对象，显示子物体总数和MD对象数量
        if (Selection.count == 1) {
            MetaDataObjCount = 0;
            TS_OBJSWithMetaData.Clear();



            Transform selectedTS = Selection.gameObjects[0].transform;
            //获取到当前对象以及其下所有的子对象
            Transform[] AllChildTS = selectedTS.GetComponentsInChildren<Transform>(true);
         
           //遍历所有子物体，尝试找到MetaData的对象
            foreach (var i in AllChildTS)
            {

                if (i.gameObject.GetComponent<Metadata>())
                {
                    MetaDataObjCount++;
                    TS_OBJSWithMetaData.Add(i);

                    }
                
                }

            GUILayout.Label("这个对象有" + AllChildTS.Length + "个子物体"+"其中有"+MetaDataObjCount+"个MD对象");

          

            //第一步：清理不可见的对象

            GUILayout.Label("第一步：清理无模型对象。规则：如果这个子对象不再包含自己的子对象，且也没有renderer组件");

            if (GUILayout.Button("查询不可见对象信息", GUILayout.Width(200)))
            {

                TS_OBJNeedToClear.Clear();

                for (int i = 0; i < AllChildTS.Length; i++)
                {

                    if ((null==AllChildTS[i].GetComponent<Renderer>())&& AllChildTS[i].childCount<1)
                    {
                        

                        TS_OBJNeedToClear.Add(AllChildTS[i]);
                    }
                }
                Debug.Log("要清理的对象数量为" + TS_OBJNeedToClear.Count);
            }
            
            if (GUILayout.Button("清理不可见对象", GUILayout.Width(200)))
            {


                foreach (var i in TS_OBJNeedToClear)
                {
                    Debug.Log(i.name + "被清理了。");
                    DestroyImmediate(i.gameObject);
                    
                }
            }

            //第二步：根据delete、isReappear清理

            GUILayout.Label("第二步：清理链接体。规则：如果这个子对象tsDelete为yes且不包含reappear字段");

            if (GUILayout.Button("查询需要ISDelete的链接体，保留reappear", GUILayout.Width(200)))
            {
                TS_OBJNeedToClear.Clear();

                foreach (var MetaOBJ in TS_OBJSWithMetaData)
                {
                    if (MetaOBJ.GetComponent<Metadata>().containsProperty("tsDelete"))
                    {

                        if (MetaOBJ.gameObject.GetComponent<Metadata>().getProperty("tsDelete") =="yes")
                        {

                            if (!MetaOBJ.GetComponent<Metadata>().containsProperty("tsReappear")) {


                                TS_OBJNeedToClear.Add(MetaOBJ);

                            }

                        
                        }
                    }

                 

                }

                Debug.Log("tsDelete为yes且不包含reappear字段的对象有" + TS_OBJNeedToClear.Count);
            }

            if (GUILayout.Button("清理这些对象", GUILayout.Width(200)))
            {


                foreach (var i in TS_OBJNeedToClear)
                {

                    Debug.Log(i.name + "被清理了。");
                    DestroyImmediate(i.gameObject);
                }
            }



            //第三步,关键词tsIsShow，若为NO，则删除这些对象

            GUILayout.Label("第三步：清理片体、辅助对象。关键词tsIsShow，若为no，则删除这些对象");

            if (GUILayout.Button("查询tsShow状态，若为no，则准备清理", GUILayout.Width(200)))
            {
                TS_OBJNeedToClear.Clear();
                foreach (var MetaOBJ in TS_OBJSWithMetaData) 
                {

                    if (MetaOBJ.GetComponent<Metadata>().containsProperty("tsIsShow"))
                    {

                        if (MetaOBJ.gameObject.GetComponent<Metadata>().getProperty("tsIsShow") == "no")
                        {

                            TS_OBJNeedToClear.Add(MetaOBJ);
                            Debug.Log("包含" + "tsIsShow" + "且值为no的对象是" + MetaOBJ.name);
                        }
                    }

                }
            }

            if (GUILayout.Button("清理这些对象", GUILayout.Width(200)))
            {


                foreach (var i in TS_OBJNeedToClear)
                {

                    Debug.Log(i.name + "被清理了。");
                    DestroyImmediate(i.gameObject);
                }
            }

            //功能四，替换材质为统一材质
            GUILayout.Label("功能四：材质替换，");

            _LoacalMatarialName = EditorGUILayout.TextField("本地材质库名称", _LoacalMatarialName);
            _MDMaterialPropertyName = EditorGUILayout.TextField("属性名称", _MDMaterialPropertyName);
            _MDMaterialValueName = EditorGUILayout.TextField("CAD材质代号", _MDMaterialValueName);

            if (GUILayout.Button("查询符合要求的材质", GUILayout.Width(200)))
            {
                TS_OBJNeedToChangeMaterial.Clear();
                foreach (var MetaOBJ in TS_OBJSWithMetaData)
                {
                    if (MetaOBJ.GetComponent<Metadata>().containsProperty(_MDMaterialPropertyName))
                    {
                        if (MetaOBJ.gameObject.GetComponent<Metadata>().getProperty(_MDMaterialPropertyName) == _MDMaterialValueName)
                        {
                            TS_OBJNeedToChangeMaterial.Add(MetaOBJ);
                            Debug.Log("包含" + _MDMaterialPropertyName + "且值为" + _MDMaterialValueName + MetaOBJ.name);
                        }
                    }
                }
                Debug.Log("等待替换的对象数量是" + TS_OBJNeedToChangeMaterial.Count);

            }

            if (GUILayout.Button("批量替换为本地材质", GUILayout.Width(200)))
            {

                foreach (var i in TS_OBJNeedToChangeMaterial)
                {
                    i.GetComponent<Renderer>().material = Resources.Load(_LoacalMatarialName) as Material;
                }
            }

            if (GUILayout.Button("一键替换所有材质,无视Meta", GUILayout.Width(200)))
            {

                foreach (var MetaOBJ in AllChildTS)
                {
                    
                  if (MetaOBJ.GetComponent<Renderer>()) 
                  {

                        //由于场景中的部件有维子材质。因此需要先做一个数组，把数组赋予给原Meshrenderer组件。

                        Material[] localmaterials = new Material[MetaOBJ.GetComponent<MeshRenderer>().sharedMaterials.Length];
                        for (int i = 0; i < localmaterials.Length; i++)
                        {
                            localmaterials[i] = Resources.Load(_LoacalMatarialName) as Material;
                        }

                        //MetaOBJ.GetComponent<MeshRenderer>().material= Resources.Load(_LoacalMatarialName) as Material;
                        MetaOBJ.GetComponent<MeshRenderer>().materials = localmaterials;
                    }
                    
                }
            }


            //查询所有带有切割材质的对象，添加面片追踪组件
            if (GUILayout.Button("为切割材质对象添加切面追踪组件", GUILayout.Width(200)))
            {

                foreach (var MetaOBJ in AllChildTS)
                {

                    if (MetaOBJ.GetComponent<Renderer>())
                    {

                        Material[] materials;
                        //获取材质名称，如果是包含切割类材质则需要新的组件

                        materials= MetaOBJ.GetComponent<MeshRenderer>().sharedMaterials;

                        foreach (var mt in materials)
                        {
                            if (mt.name.Contains("sectionBy2"))
                            {

                                if (!MetaOBJ.GetComponent<CutPlanerTrackerCrossed>())
                                {

                                    MetaOBJ.gameObject.AddComponent<CutPlanerTrackerCrossed>();
                                }

                            }


                        }

                    }

                }
            }

        }

    }

    private void OnSelectionChange()
    {
        this.Repaint();
    }

    void OnInspectorUpdate()
    {
        //Debug.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        //this.Repaint();
    }
}
