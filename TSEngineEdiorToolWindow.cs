using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pixyz.ImportSDK;

public class TSEngineEdiorToolWindow :EditorWindow
{

    //�ýű���Ҫ�����������ɼ����������滻���ʡ�
   
    //��������
    
    [MenuItem("TSAeroEngineTool/TSEngineCADTool %t", false, 11)]

     static void OpenCADToolWindow() {

        Rect wr = new Rect(0, 0, 500, 800);
        TSEngineEdiorToolWindow window = (TSEngineEdiorToolWindow)EditorWindow.GetWindowWithRect(typeof(TSEngineEdiorToolWindow), wr, true, "TSEngineCADTool-���������滻");
        window.Show();
    
    }


    //���¿�ʼ׫д������Ϣ
    //����Ժlogo
    private Texture _texture;

    //�����滻�������ñ�����MD�ļ���Ĳ����ֶ����ơ�ֵ���Լ���Ӧ�ı��ز�������
    private string _MDMaterialPropertyName= "tsMaterial";
    private string _MDMaterialValueName="������CAD���ʱ��";
    private string _LoacalMatarialName="������Ҫ�滻�ı��ز�������";
    //����ء���ŷ��������ġ������滻���ʵĶ���
    List<Transform> TS_OBJNeedToChangeMaterial = new List<Transform>();
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
        if (Selection.count == 1) {
            MetaDataObjCount = 0;
            TS_OBJSWithMetaData.Clear();



            Transform selectedTS = Selection.gameObjects[0].transform;
            //��ȡ����ǰ�����Լ��������е��Ӷ���
            Transform[] AllChildTS = selectedTS.GetComponentsInChildren<Transform>(true);
         
           //�������������壬�����ҵ�MetaData�Ķ���
            foreach (var i in AllChildTS)
            {

                if (i.gameObject.GetComponent<Metadata>())
                {
                    MetaDataObjCount++;
                    TS_OBJSWithMetaData.Add(i);

                    }
                
                }

            GUILayout.Label("���������" + AllChildTS.Length + "��������"+"������"+MetaDataObjCount+"��MD����");

          

            //��һ���������ɼ��Ķ���

            GUILayout.Label("��һ����������ģ�Ͷ��󡣹����������Ӷ����ٰ����Լ����Ӷ�����Ҳû��renderer���");

            if (GUILayout.Button("��ѯ���ɼ�������Ϣ", GUILayout.Width(200)))
            {

                TS_OBJNeedToClear.Clear();

                for (int i = 0; i < AllChildTS.Length; i++)
                {

                    if ((null==AllChildTS[i].GetComponent<Renderer>())&& AllChildTS[i].childCount<1)
                    {
                        

                        TS_OBJNeedToClear.Add(AllChildTS[i]);
                    }
                }
                Debug.Log("Ҫ����Ķ�������Ϊ" + TS_OBJNeedToClear.Count);
            }
            
            if (GUILayout.Button("�����ɼ�����", GUILayout.Width(200)))
            {


                foreach (var i in TS_OBJNeedToClear)
                {
                    Debug.Log(i.name + "�������ˡ�");
                    DestroyImmediate(i.gameObject);
                    
                }
            }

            //�ڶ���������delete��isReappear����

            GUILayout.Label("�ڶ��������������塣�����������Ӷ���tsDeleteΪyes�Ҳ�����reappear�ֶ�");

            if (GUILayout.Button("��ѯ��ҪISDelete�������壬����reappear", GUILayout.Width(200)))
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

                Debug.Log("tsDeleteΪyes�Ҳ�����reappear�ֶεĶ�����" + TS_OBJNeedToClear.Count);
            }

            if (GUILayout.Button("������Щ����", GUILayout.Width(200)))
            {


                foreach (var i in TS_OBJNeedToClear)
                {

                    Debug.Log(i.name + "�������ˡ�");
                    DestroyImmediate(i.gameObject);
                }
            }



            //������,�ؼ���tsIsShow����ΪNO����ɾ����Щ����

            GUILayout.Label("������������Ƭ�塢�������󡣹ؼ���tsIsShow����Ϊno����ɾ����Щ����");

            if (GUILayout.Button("��ѯtsShow״̬����Ϊno����׼������", GUILayout.Width(200)))
            {
                TS_OBJNeedToClear.Clear();
                foreach (var MetaOBJ in TS_OBJSWithMetaData) 
                {

                    if (MetaOBJ.GetComponent<Metadata>().containsProperty("tsIsShow"))
                    {

                        if (MetaOBJ.gameObject.GetComponent<Metadata>().getProperty("tsIsShow") == "no")
                        {

                            TS_OBJNeedToClear.Add(MetaOBJ);
                            Debug.Log("����" + "tsIsShow" + "��ֵΪno�Ķ�����" + MetaOBJ.name);
                        }
                    }

                }
            }

            if (GUILayout.Button("������Щ����", GUILayout.Width(200)))
            {


                foreach (var i in TS_OBJNeedToClear)
                {

                    Debug.Log(i.name + "�������ˡ�");
                    DestroyImmediate(i.gameObject);
                }
            }

            //�����ģ��滻����Ϊͳһ����
            GUILayout.Label("�����ģ������滻��");

            _LoacalMatarialName = EditorGUILayout.TextField("���ز��ʿ�����", _LoacalMatarialName);
            _MDMaterialPropertyName = EditorGUILayout.TextField("��������", _MDMaterialPropertyName);
            _MDMaterialValueName = EditorGUILayout.TextField("CAD���ʴ���", _MDMaterialValueName);

            if (GUILayout.Button("��ѯ����Ҫ��Ĳ���", GUILayout.Width(200)))
            {
                TS_OBJNeedToChangeMaterial.Clear();
                foreach (var MetaOBJ in TS_OBJSWithMetaData)
                {
                    if (MetaOBJ.GetComponent<Metadata>().containsProperty(_MDMaterialPropertyName))
                    {
                        if (MetaOBJ.gameObject.GetComponent<Metadata>().getProperty(_MDMaterialPropertyName) == _MDMaterialValueName)
                        {
                            TS_OBJNeedToChangeMaterial.Add(MetaOBJ);
                            Debug.Log("����" + _MDMaterialPropertyName + "��ֵΪ" + _MDMaterialValueName + MetaOBJ.name);
                        }
                    }
                }
                Debug.Log("�ȴ��滻�Ķ���������" + TS_OBJNeedToChangeMaterial.Count);

            }

            if (GUILayout.Button("�����滻Ϊ���ز���", GUILayout.Width(200)))
            {

                foreach (var i in TS_OBJNeedToChangeMaterial)
                {
                    i.GetComponent<Renderer>().material = Resources.Load(_LoacalMatarialName) as Material;
                }
            }

            if (GUILayout.Button("һ���滻���в���,����Meta", GUILayout.Width(200)))
            {

                foreach (var MetaOBJ in AllChildTS)
                {
                    
                  if (MetaOBJ.GetComponent<Renderer>()) 
                  {

                        //���ڳ����еĲ�����ά�Ӳ��ʡ������Ҫ����һ�����飬�����鸳���ԭMeshrenderer�����

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


            //��ѯ���д����и���ʵĶ��������Ƭ׷�����
            if (GUILayout.Button("Ϊ�и���ʶ����������׷�����", GUILayout.Width(200)))
            {

                foreach (var MetaOBJ in AllChildTS)
                {

                    if (MetaOBJ.GetComponent<Renderer>())
                    {

                        Material[] materials;
                        //��ȡ�������ƣ�����ǰ����и����������Ҫ�µ����

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
        //Debug.Log("�������ĸ���");
        //���￪�����ڵ��ػ棬��Ȼ������Ϣ����ˢ��
        //this.Repaint();
    }
}
