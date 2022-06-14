//CopyRight:THIAE Author:Peter
/*
作为航发CAD结构模型清理流程之一，该脚本实现对装配节点整理，进一步的还可实现自动批量播放动画，形成生长的炫酷视觉效果。

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pixyz.ImportSDK;
using System.Linq;

public class TSEngineEdiorToolCombine : EditorWindow
{


    //窗口启动
    
    [MenuItem("TSAeroEngineTool/TSEngineCADCombine", false, 11)]

     static void OpenCADToolWindow() {

        Rect wr = new Rect(0, 0, 800, 800);
        TSEngineEdiorToolCombine window = (TSEngineEdiorToolCombine)EditorWindow.GetWindowWithRect(typeof(TSEngineEdiorToolCombine), wr, true, "TSEngineCADTool-整理bom层级信息");
        window.Show();
    
    }


    //以下开始撰写窗口信息
    //航发院logo
    private Texture _texture;

    Transform selectedTS;


    //材质替换功能所用变量：MD文件里的材质字段名称、值，以及对应的本地材质名称
    private string _MDMaterialPropertyName= "tsMaterial";
    private string _MDMaterialValueName="请输入CAD材质编号";
    private string _LoacalMatarialName="请输入要替换的本地材质名称";

    //按名称查找功能所用变量
    private string _NameKeyWord = "输入名称";
    private string _GroupName = "存放到哪个组";
    private float _BiasValue = 0.01f;





    //对象池、存放符合条件的、即将替换材质的对象
    List<Transform> TS_OBJNeedToChangeMaterial = new List<Transform>();


    //对象池、存放符合条件的、用于下一步操作的对象
    List<Transform> TS_OBJNeedToOperate = new List<Transform>();
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
        //-----------单个对象的操作----------------
        if (Selection.count == 1)
        {
            MetaDataObjCount = 0;
            TS_OBJSWithMetaData.Clear();



            selectedTS = Selection.gameObjects[0].transform;
            //获取到当前对象以及其下所有的子对象
            Transform[] AllChildTS = selectedTS.GetComponentsInChildren<Transform>(true);

            //遍历所有子物体，尝试找到MetaData的对象,加入到objwithMetaData的表单中。
            foreach (var i in AllChildTS)
            {

                if (i.gameObject.GetComponent<Metadata>())
                {
                    MetaDataObjCount++;
                    TS_OBJSWithMetaData.Add(i);

                }

            }

            GUILayout.Label("这个对象有" + AllChildTS.Length + "个子物体" + "其中有" + MetaDataObjCount + "个MD对象");
            GUILayout.Label("世界坐标" + selectedTS.position + "本地坐标" + selectedTS.localPosition + "世界角" + selectedTS.eulerAngles + "本地角" + selectedTS.localScale);
            GUILayout.Label("渲染中心点是" + GettheCenterPos(selectedTS) + "包围盒yz体积是" + GettheBoundSize(selectedTS) + "最远" + GettheMaxPointDistance(selectedTS));

            //第一步：根据名称整理分组

            GUILayout.Label("第一步：根据关键词，查找对象");
            _NameKeyWord = EditorGUILayout.TextField("名字关键词", _NameKeyWord);
            _GroupName = EditorGUILayout.TextField("输入组名", _GroupName);

            if (GUILayout.Button("查询、搜索含有此关键词的对象", GUILayout.Width(200)))
            {

                TS_OBJNeedToOperate.Clear();

                for (int i = 0; i < AllChildTS.Length; i++)
                {

                    if ((AllChildTS[i].name.Contains(_NameKeyWord)))
                    {


                        TS_OBJNeedToOperate.Add(AllChildTS[i]);
                    }
                }

                Debug.Log("查找到的对象数量为" + TS_OBJNeedToOperate.Count);
            }


            //对找到的这些部件进行分组操作 
            if (GUILayout.Button("操作这些对象，打组", GUILayout.Width(200)))
            {
                Transform TS_Group;

                if (GameObject.Find(_GroupName))
                {

                    TS_Group = GameObject.Find(_GroupName).transform;

                }
                else
                {

                    TS_Group = new GameObject(_GroupName).transform;


                }

                TS_Group.SetParent(selectedTS);

                foreach (var i in TS_OBJNeedToOperate)
                {
                    i.SetParent(TS_Group);
                }
            }


            //第二步：查找该对象附近的类似对象。 
            GUILayout.Label("第二步：选择附近的相似对象// 规则：1.世界坐标的Z轴一致；2.拥有同一个父对象");



            if (GUILayout.Button("查找附近相似对象", GUILayout.Width(200)))
            {
                //先找到父对象
                TS_OBJNeedToOperate.Clear();
                var ParentofSelected = selectedTS.parent;
                for (int i = 0; i < ParentofSelected.childCount; i++)
                {
                    //先判定,这些兄弟对象是不是都在一个X平面上
                    if (Mathf.Abs(GettheCenterPos(ParentofSelected.GetChild(i)).x - GettheCenterPos(selectedTS).x) < 0.00001f)
                    {


                        TS_OBJNeedToOperate.Add(ParentofSelected.GetChild(i));

                        /*if ((GettheBoundSize(ParentofSelected.GetChild(i))- GettheBoundSize(selectedTS)) < 0.001f)
                        {

                            //再判定包围盒X宽度是不是一致
                           

                        }*/


                    }

                }

                Debug.Log("同级别兄弟数量" + ParentofSelected.childCount);
                Debug.Log("其中符合要求的数量为" + TS_OBJNeedToOperate.Count);

            }



            if (GUILayout.Button("试图选中这些对象", GUILayout.Width(200)))
            {
                Debug.Log("尝试选中对象数量是" + TS_OBJNeedToOperate.Count);
                GameObject[] newSelection = new GameObject[TS_OBJNeedToOperate.Count];
                for (int i = 0; i < TS_OBJNeedToOperate.Count; i++)
                {
                    newSelection[i] = TS_OBJNeedToOperate[i].gameObject;
                }

                Selection.objects = newSelection;

            }


            if (GUILayout.Button("操作这些对象，按照选定对象名称打组", GUILayout.Width(200)))
            {
                Transform TS_Group;
                string NewGroupName;
                NewGroupName = "Similar_Group" + selectedTS.name;
                if (GameObject.Find(NewGroupName))
                {

                    NewGroupName += GettheCenterPos(selectedTS);
                    TS_Group = new GameObject(NewGroupName).transform;

                }
                else
                {

                    TS_Group = new GameObject(NewGroupName).transform;


                }

                TS_Group.SetParent(selectedTS.parent);

                foreach (var i in TS_OBJNeedToOperate)
                {
                    i.SetParent(TS_Group);
                }

                /*Transform[] groupTS = new Transform[1];
                groupTS[0] = TS_Group;

                Selection.objects = groupTS;*/

            }

            GUILayout.Label("-------------第三步：自动化分组、排序功能---------");

            if (GUILayout.Button("排序:根据中心点,沿X轴从前到后"))
            {

                int itemIndex = 0;
                //获取所有子对象的数组（不包含孙对象）
                Transform[] childrenOfSelection = new Transform[selectedTS.childCount];
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection[i] = selectedTS.GetChild(i);
                }


                //利用linq的排序功能处理这些对象
                var orderedChildren = childrenOfSelection.OrderByDescending(c => (GettheCenterPos(c).x));

                foreach (var item in orderedChildren)
                {


                    //Debug.Log(item.name+item.GetSiblingIndex().ToString());

                    item.SetSiblingIndex(itemIndex);
                   // item.name = itemIndex.ToString() + "ByXPos" + "_" + item.name;
                    itemIndex++;
                }
            }

            if (GUILayout.Button("排序:根据最前点，沿X轴从前到后"))
            {

                int itemIndex = 0;
                //获取所有子对象的数组（不包含孙对象）
                Transform[] childrenOfSelection = new Transform[selectedTS.childCount];
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection[i] = selectedTS.GetChild(i);
                }


                //利用linq的排序功能处理这些对象
                var orderedChildren = childrenOfSelection.OrderByDescending(c => (GettheMaxPos(c).x));

                foreach (var item in orderedChildren)
                {


                    //Debug.Log(item.name+item.GetSiblingIndex().ToString());


                    item.SetSiblingIndex(itemIndex);
                   
                    //item.name = itemIndex.ToString() + "ByMinXPos" + "_" + GettheMaxPos(item).x + "_" + "LV1";
                    itemIndex++;
                }
            }

            if (GUILayout.Button("排序:根据最外点到圆心距离，从内向外"))
            {

                int itemIndex = 0;
                //获取所有子对象的数组（不包含孙对象）
                Transform[] childrenOfSelection = new Transform[selectedTS.childCount];
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection[i] = selectedTS.GetChild(i);
                }


                //利用linq的排序功能处理这些对象
                var orderedChildren = childrenOfSelection.OrderBy(c => (GettheMaxPointDistance(c)));

                foreach (var item in orderedChildren)
                {


                    //Debug.Log(item.name+item.GetSiblingIndex().ToString());

                    item.SetSiblingIndex(itemIndex);
                   // item.name = itemIndex.ToString() + "ByCenter" + "_" + item.name;
                    itemIndex++;
                }
            }




            if (GUILayout.Button("查询重名对象,并分组"))
            {

                Transform tempGroup = null;

                List<Transform> childrenOfSelection = new List<Transform>();
                List<string> namesOfchildren = new List<string>();
                childrenOfSelection.Clear();
                namesOfchildren.Clear();
                //获取所有子对象的数组（不包含孙对象）
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection.Add(selectedTS.GetChild(i));

                }

                //添加PartData组件,计算各自的同名兄弟数量

                foreach (var i in childrenOfSelection)
                {
                    PartData ipartData;

                    if (i.GetComponent<PartData>())
                    {

                        ipartData = i.GetComponent<PartData>();

                        ipartData.getSameNameCount();
                    }
                    else
                    {
                        ipartData = i.gameObject.AddComponent<PartData>();
                        ipartData.getSameNameCount();
                    }


                }

                //再次遍历数组，同名兄弟数量较大，新建"SameName_Group"组，放入其中
                foreach (var i in childrenOfSelection)
                {
                    PartData ipartData;


                    ipartData = i.GetComponent<PartData>();

                    if (ipartData.SameNameCount > 1)
                    {

                        if (GameObject.Find("SameName_Group" + i.name))
                        {

                            tempGroup = GameObject.Find("SameName_Group" + i.name).transform;
                            tempGroup.SetParent(selectedTS);

                            i.transform.SetParent(tempGroup);


                        }
                        else
                        {


                            tempGroup = new GameObject("SameName_Group" + i.name).transform;
                            tempGroup.SetParent(selectedTS);

                            i.transform.SetParent(tempGroup);


                        }


                    }



                }



            }


            if (GUILayout.Button("查询相似对象,并分组"))
            {

                Transform tempGroup = null;

                List<Transform> childrenOfSelection = new List<Transform>();
                List<string> namesOfchildren = new List<string>();
                childrenOfSelection.Clear();
                namesOfchildren.Clear();
                //获取所有子对象的数组（不包含孙对象）
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection.Add(selectedTS.GetChild(i));

                }

                //添加PartData组件,计算各自相似兄弟数量

                foreach (var i in childrenOfSelection)
                {
                    PartData ipartData;

                    if (i.GetComponent<PartData>())
                    {

                        ipartData = i.GetComponent<PartData>();

                        ipartData.getSimilarCount();
                        ipartData.getTheRenderCenter();
                    }
                    else
                    {
                        ipartData = i.gameObject.AddComponent<PartData>();
                        ipartData.getSimilarCount();
                        ipartData.getTheRenderCenter();
                    }


                }

                //再次遍历数组，相似兄弟数量较大，新建"Similar_Group"组，放入其中
                foreach (var i in childrenOfSelection)
                {
                    PartData ipartData;
                    float CenterPosTo001;

                    ipartData = i.GetComponent<PartData>();

                    //如果这个对象，在0001的精度下，有相似兄弟
                    CenterPosTo001 = (Mathf.RoundToInt(ipartData.RendererCenterPos.x * 1000.0f)) / 1000.0f;
                    Debug.Log("CenterPosTO001:" + CenterPosTo001 +"---" +i.name);

                    if (ipartData.SimilarBrotherCount > 1)
                    {
                        
                        if (GameObject.Find("Similar_Group" + CenterPosTo001))
                        {

                            tempGroup = GameObject.Find("Similar_Group" + CenterPosTo001).transform;
                            tempGroup.SetParent(selectedTS);

                            i.transform.SetParent(tempGroup);


                        }
                        else
                        {


                            tempGroup = new GameObject("Similar_Group" + CenterPosTo001).transform;
                            tempGroup.SetParent(selectedTS);

                            i.transform.SetParent(tempGroup);


                        }


                    }



                }



            }

            //X轴切片分析功能，分析发动机包围盒。在特定步长内检查切片分组。

            GUILayout.Label("-------------第四步：添加动画组件标记---------");

            if (GUILayout.Button("为子对象批量添加动画组件AT"))
            {

                Transform tempGroup = null;

                List<Transform> childrenOfSelection = new List<Transform>();
                List<string> namesOfchildren = new List<string>();
                childrenOfSelection.Clear();
                namesOfchildren.Clear();
                //获取所有子对象的数组（不包含孙对象）
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection.Add(selectedTS.GetChild(i));

                }

                //添加AutoTween组件；

                foreach (var i in childrenOfSelection)
                {
                    AutoTween AT;

                    if (i.GetComponent<AutoTween>())
                    {

                        AT = i.GetComponent<AutoTween>();

                    }
                    else
                    {
                        AT = i.gameObject.AddComponent<AutoTween>();

                    }


                }


            }
            if (GUILayout.Button("单独添加动画组件AT"))
            {

                
                    AutoTween AT;

                    if (selectedTS.GetComponent<AutoTween>())
                    {

                        AT = selectedTS.GetComponent<AutoTween>();

                    }
                    else
                    {
                        AT = selectedTS.gameObject.AddComponent<AutoTween>();

                    }


            }


        }
        else {



            //把当前选中的多个对象放到一个组当中
            GUILayout.Label("当前选中了对象有"+Selection.count+"个");

            if (GUILayout.Button("打组")) {

                //新建一个组
                var newGroup = new GameObject("Unnamed_new_group");
                newGroup.transform.SetParent(Selection.gameObjects[0].transform.parent);
                //遍历已选对象，依次放入
                foreach (var item in Selection.gameObjects)
                {

                    item.transform.SetParent(newGroup.transform);

                }
 
            
            }

            //为多个对象添加AT组件

            if (GUILayout.Button("添加AT组件")) 
            {

                foreach (var item in Selection.gameObjects)
                {

                    if (item.GetComponent<AutoTween>())
                    {
                        //return;
                    }
                    else
                    {

                        item.AddComponent<AutoTween>();

                    }

                }


            }
               

        }
    }

    private void OnSelectionChange()
    {
       // this.Repaint();
    }

    void OnInspectorUpdate()
    {
        //Debug.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
    }

    //------------------------------------------            ------------分离功能区-----------                ---------------------------

    //得到模型的中心点
    Vector3 GettheCenterPos() {

        Vector3 centerpos=Vector3.zero;

        Renderer[] renderers = selectedTS.GetComponentsInChildren<Renderer>(true);


        foreach (Renderer child in renderers)
        {
            centerpos += child.bounds.center;
        }
        centerpos /= renderers.Length;



        return centerpos;
    
    
    
    }
    
    //得到模型的中心点,重构
    Vector3 GettheCenterPos(Transform Target) {

        Vector3 centerpos=Vector3.zero;

        Renderer[] renderers = Target.GetComponentsInChildren<Renderer>(true);


        foreach (Renderer child in renderers)
        {
            centerpos += child.bounds.center;
        }
        centerpos /= renderers.Length;



        return centerpos;
    
    
    
    }

    //得到模型的包围盒体积
    float GettheBoundSize(Transform Target)
    {

        float BoundsSize;
        Bounds bounds = new Bounds(GettheCenterPos(Target), Vector3.zero);

        Renderer[] renderers = Target.GetComponentsInChildren<Renderer>(true);


        foreach (Renderer child in renderers)
        {
            bounds.Encapsulate(child.bounds);
        }


        BoundsSize = Mathf.Abs( bounds.size.y * bounds.size.z);

        return BoundsSize;



    } 
    
    //得到模型的包围盒体积V3
    Vector3 GettheBoundSizeV3(Transform Target)
    {

        Vector3 BoundsSizeV3;
        Bounds bounds = new Bounds(GettheCenterPos(Target), Vector3.zero);

        Renderer[] renderers = Target.GetComponentsInChildren<Renderer>(true);


        foreach (Renderer child in renderers)
        {
            bounds.Encapsulate(child.bounds);
        }




        return bounds.size;



    }

    //得到模型最远点距离圆心的距离
    float GettheMaxPointDistance(Transform Target)
    {

        float maxPointDistance=0;
        Vector3 centerPoint;
        Vector2 PointYZ;
        Bounds bounds = new Bounds(GettheCenterPos(Target), Vector3.zero);

        Renderer[] renderers = Target.GetComponentsInChildren<Renderer>(true);


        foreach (Renderer child in renderers)
        {
            bounds.Encapsulate(child.bounds);
        }

        centerPoint = bounds.center;
        PointYZ = new Vector2(Mathf.Abs(centerPoint.y), Mathf.Abs(centerPoint.z));
        PointYZ += new Vector2(bounds.extents.y , bounds.extents.z);

        maxPointDistance = PointYZ.magnitude;

        //BoundsSize = Mathf.Abs(bounds.size.y * bounds.size.z);

        return maxPointDistance;


    }


    public Vector3 GettheMaxPos(Transform Target)
    {

        Vector3 _MaxPos;

        Bounds bounds = new Bounds(GettheCenterPos(Target.transform), Vector3.zero);

        Renderer[] renderers = Target.transform.GetComponentsInChildren<Renderer>(true);


        foreach (Renderer child in renderers)
        {
            bounds.Encapsulate(child.bounds);
        }

        _MaxPos = bounds.max;

       
        return _MaxPos;


    }

}
