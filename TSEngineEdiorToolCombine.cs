//CopyRight:THIAE Author:Peter
/*
��Ϊ����CAD�ṹģ����������֮һ���ýű�ʵ�ֶ�װ��ڵ�������һ���Ļ���ʵ���Զ��������Ŷ������γ��������ſ��Ӿ�Ч����

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pixyz.ImportSDK;
using System.Linq;

public class TSEngineEdiorToolCombine : EditorWindow
{


    //��������
    
    [MenuItem("TSAeroEngineTool/TSEngineCADCombine", false, 11)]

     static void OpenCADToolWindow() {

        Rect wr = new Rect(0, 0, 800, 800);
        TSEngineEdiorToolCombine window = (TSEngineEdiorToolCombine)EditorWindow.GetWindowWithRect(typeof(TSEngineEdiorToolCombine), wr, true, "TSEngineCADTool-����bom�㼶��Ϣ");
        window.Show();
    
    }


    //���¿�ʼ׫д������Ϣ
    //����Ժlogo
    private Texture _texture;

    Transform selectedTS;


    //�����滻�������ñ�����MD�ļ���Ĳ����ֶ����ơ�ֵ���Լ���Ӧ�ı��ز�������
    private string _MDMaterialPropertyName= "tsMaterial";
    private string _MDMaterialValueName="������CAD���ʱ��";
    private string _LoacalMatarialName="������Ҫ�滻�ı��ز�������";

    //�����Ʋ��ҹ������ñ���
    private string _NameKeyWord = "��������";
    private string _GroupName = "��ŵ��ĸ���";
    private float _BiasValue = 0.01f;





    //����ء���ŷ��������ġ������滻���ʵĶ���
    List<Transform> TS_OBJNeedToChangeMaterial = new List<Transform>();


    //����ء���ŷ��������ġ�������һ�������Ķ���
    List<Transform> TS_OBJNeedToOperate = new List<Transform>();
    //��ǰ�������Ӷ�����������в��ʵ�����
    List<string> Material_ObjectContained = new List<string>();

    //ͳ����ѡ����������������MetaData���������
    private int SelectedCount;
    int MetaDataObjCount = 0;
    //ӵ��MD�Ķ����
    List<Transform> TS_OBJSWithMetaData = new List<Transform>();

    //��������Ķ����
    List<Transform> TS_OBJNeedToClear = new List<Transform>();




    public void Awake()
    {
        _texture = Resources.Load("logo") as Texture;
    }

    private void OnGUI()
    {
        //�����ö���logo
        GUILayout.Label(_texture);


        //���ڵ�ǰѡ��������Ϣ
        GUILayout.Label("��ѡ����" + Selection.count + "������");

        //����û�ֻѡ����һ��������ʾ������������MD��������
        //-----------��������Ĳ���----------------
        if (Selection.count == 1)
        {
            MetaDataObjCount = 0;
            TS_OBJSWithMetaData.Clear();



            selectedTS = Selection.gameObjects[0].transform;
            //��ȡ����ǰ�����Լ��������е��Ӷ���
            Transform[] AllChildTS = selectedTS.GetComponentsInChildren<Transform>(true);

            //�������������壬�����ҵ�MetaData�Ķ���,���뵽objwithMetaData�ı��С�
            foreach (var i in AllChildTS)
            {

                if (i.gameObject.GetComponent<Metadata>())
                {
                    MetaDataObjCount++;
                    TS_OBJSWithMetaData.Add(i);

                }

            }

            GUILayout.Label("���������" + AllChildTS.Length + "��������" + "������" + MetaDataObjCount + "��MD����");
            GUILayout.Label("��������" + selectedTS.position + "��������" + selectedTS.localPosition + "�����" + selectedTS.eulerAngles + "���ؽ�" + selectedTS.localScale);
            GUILayout.Label("��Ⱦ���ĵ���" + GettheCenterPos(selectedTS) + "��Χ��yz�����" + GettheBoundSize(selectedTS) + "��Զ" + GettheMaxPointDistance(selectedTS));

            //��һ�������������������

            GUILayout.Label("��һ�������ݹؼ��ʣ����Ҷ���");
            _NameKeyWord = EditorGUILayout.TextField("���ֹؼ���", _NameKeyWord);
            _GroupName = EditorGUILayout.TextField("��������", _GroupName);

            if (GUILayout.Button("��ѯ���������д˹ؼ��ʵĶ���", GUILayout.Width(200)))
            {

                TS_OBJNeedToOperate.Clear();

                for (int i = 0; i < AllChildTS.Length; i++)
                {

                    if ((AllChildTS[i].name.Contains(_NameKeyWord)))
                    {


                        TS_OBJNeedToOperate.Add(AllChildTS[i]);
                    }
                }

                Debug.Log("���ҵ��Ķ�������Ϊ" + TS_OBJNeedToOperate.Count);
            }


            //���ҵ�����Щ�������з������ 
            if (GUILayout.Button("������Щ���󣬴���", GUILayout.Width(200)))
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


            //�ڶ��������Ҹö��󸽽������ƶ��� 
            GUILayout.Label("�ڶ�����ѡ�񸽽������ƶ���// ����1.���������Z��һ�£�2.ӵ��ͬһ��������");



            if (GUILayout.Button("���Ҹ������ƶ���", GUILayout.Width(200)))
            {
                //���ҵ�������
                TS_OBJNeedToOperate.Clear();
                var ParentofSelected = selectedTS.parent;
                for (int i = 0; i < ParentofSelected.childCount; i++)
                {
                    //���ж�,��Щ�ֵܶ����ǲ��Ƕ���һ��Xƽ����
                    if (Mathf.Abs(GettheCenterPos(ParentofSelected.GetChild(i)).x - GettheCenterPos(selectedTS).x) < 0.00001f)
                    {


                        TS_OBJNeedToOperate.Add(ParentofSelected.GetChild(i));

                        /*if ((GettheBoundSize(ParentofSelected.GetChild(i))- GettheBoundSize(selectedTS)) < 0.001f)
                        {

                            //���ж���Χ��X����ǲ���һ��
                           

                        }*/


                    }

                }

                Debug.Log("ͬ�����ֵ�����" + ParentofSelected.childCount);
                Debug.Log("���з���Ҫ�������Ϊ" + TS_OBJNeedToOperate.Count);

            }



            if (GUILayout.Button("��ͼѡ����Щ����", GUILayout.Width(200)))
            {
                Debug.Log("����ѡ�ж���������" + TS_OBJNeedToOperate.Count);
                GameObject[] newSelection = new GameObject[TS_OBJNeedToOperate.Count];
                for (int i = 0; i < TS_OBJNeedToOperate.Count; i++)
                {
                    newSelection[i] = TS_OBJNeedToOperate[i].gameObject;
                }

                Selection.objects = newSelection;

            }


            if (GUILayout.Button("������Щ���󣬰���ѡ���������ƴ���", GUILayout.Width(200)))
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

            GUILayout.Label("-------------���������Զ������顢������---------");

            if (GUILayout.Button("����:�������ĵ�,��X���ǰ����"))
            {

                int itemIndex = 0;
                //��ȡ�����Ӷ�������飨�����������
                Transform[] childrenOfSelection = new Transform[selectedTS.childCount];
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection[i] = selectedTS.GetChild(i);
                }


                //����linq�������ܴ�����Щ����
                var orderedChildren = childrenOfSelection.OrderByDescending(c => (GettheCenterPos(c).x));

                foreach (var item in orderedChildren)
                {


                    //Debug.Log(item.name+item.GetSiblingIndex().ToString());

                    item.SetSiblingIndex(itemIndex);
                   // item.name = itemIndex.ToString() + "ByXPos" + "_" + item.name;
                    itemIndex++;
                }
            }

            if (GUILayout.Button("����:������ǰ�㣬��X���ǰ����"))
            {

                int itemIndex = 0;
                //��ȡ�����Ӷ�������飨�����������
                Transform[] childrenOfSelection = new Transform[selectedTS.childCount];
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection[i] = selectedTS.GetChild(i);
                }


                //����linq�������ܴ�����Щ����
                var orderedChildren = childrenOfSelection.OrderByDescending(c => (GettheMaxPos(c).x));

                foreach (var item in orderedChildren)
                {


                    //Debug.Log(item.name+item.GetSiblingIndex().ToString());


                    item.SetSiblingIndex(itemIndex);
                   
                    //item.name = itemIndex.ToString() + "ByMinXPos" + "_" + GettheMaxPos(item).x + "_" + "LV1";
                    itemIndex++;
                }
            }

            if (GUILayout.Button("����:��������㵽Բ�ľ��룬��������"))
            {

                int itemIndex = 0;
                //��ȡ�����Ӷ�������飨�����������
                Transform[] childrenOfSelection = new Transform[selectedTS.childCount];
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection[i] = selectedTS.GetChild(i);
                }


                //����linq�������ܴ�����Щ����
                var orderedChildren = childrenOfSelection.OrderBy(c => (GettheMaxPointDistance(c)));

                foreach (var item in orderedChildren)
                {


                    //Debug.Log(item.name+item.GetSiblingIndex().ToString());

                    item.SetSiblingIndex(itemIndex);
                   // item.name = itemIndex.ToString() + "ByCenter" + "_" + item.name;
                    itemIndex++;
                }
            }




            if (GUILayout.Button("��ѯ��������,������"))
            {

                Transform tempGroup = null;

                List<Transform> childrenOfSelection = new List<Transform>();
                List<string> namesOfchildren = new List<string>();
                childrenOfSelection.Clear();
                namesOfchildren.Clear();
                //��ȡ�����Ӷ�������飨�����������
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection.Add(selectedTS.GetChild(i));

                }

                //���PartData���,������Ե�ͬ���ֵ�����

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

                //�ٴα������飬ͬ���ֵ������ϴ��½�"SameName_Group"�飬��������
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


            if (GUILayout.Button("��ѯ���ƶ���,������"))
            {

                Transform tempGroup = null;

                List<Transform> childrenOfSelection = new List<Transform>();
                List<string> namesOfchildren = new List<string>();
                childrenOfSelection.Clear();
                namesOfchildren.Clear();
                //��ȡ�����Ӷ�������飨�����������
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection.Add(selectedTS.GetChild(i));

                }

                //���PartData���,������������ֵ�����

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

                //�ٴα������飬�����ֵ������ϴ��½�"Similar_Group"�飬��������
                foreach (var i in childrenOfSelection)
                {
                    PartData ipartData;
                    float CenterPosTo001;

                    ipartData = i.GetComponent<PartData>();

                    //������������0001�ľ����£��������ֵ�
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

            //X����Ƭ�������ܣ�������������Χ�С����ض������ڼ����Ƭ���顣

            GUILayout.Label("-------------���Ĳ�����Ӷ���������---------");

            if (GUILayout.Button("Ϊ�Ӷ���������Ӷ������AT"))
            {

                Transform tempGroup = null;

                List<Transform> childrenOfSelection = new List<Transform>();
                List<string> namesOfchildren = new List<string>();
                childrenOfSelection.Clear();
                namesOfchildren.Clear();
                //��ȡ�����Ӷ�������飨�����������
                for (int i = 0; i < selectedTS.childCount; i++)
                {
                    childrenOfSelection.Add(selectedTS.GetChild(i));

                }

                //���AutoTween�����

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
            if (GUILayout.Button("������Ӷ������AT"))
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



            //�ѵ�ǰѡ�еĶ������ŵ�һ���鵱��
            GUILayout.Label("��ǰѡ���˶�����"+Selection.count+"��");

            if (GUILayout.Button("����")) {

                //�½�һ����
                var newGroup = new GameObject("Unnamed_new_group");
                newGroup.transform.SetParent(Selection.gameObjects[0].transform.parent);
                //������ѡ�������η���
                foreach (var item in Selection.gameObjects)
                {

                    item.transform.SetParent(newGroup.transform);

                }
 
            
            }

            //Ϊ����������AT���

            if (GUILayout.Button("���AT���")) 
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
        //Debug.Log("�������ĸ���");
        //���￪�����ڵ��ػ棬��Ȼ������Ϣ����ˢ��
        this.Repaint();
    }

    //------------------------------------------            ------------���빦����-----------                ---------------------------

    //�õ�ģ�͵����ĵ�
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
    
    //�õ�ģ�͵����ĵ�,�ع�
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

    //�õ�ģ�͵İ�Χ�����
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
    
    //�õ�ģ�͵İ�Χ�����V3
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

    //�õ�ģ����Զ�����Բ�ĵľ���
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
