using Assimp;
using Assimp.Configs;
using Crosstales.FB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AssImportTest : MonoBehaviour
{
    public Text textRef;
    private CustomAssetImporter customModelImporter;
    public UnityEngine.Material matRef;
    
    public InputField scaleInput;

    public GameObject currentImportedModel;
    public Vector3 importedModelOrignalScale;

    public Toggle normalsRecalculate;
    public Toggle tangentsRecalculate;

    // Use this for initialization
    void Start ()
    {
        customModelImporter = new CustomAssetImporter();
        textRef.text = "AssImp : Initialized : " + Assimp.AssimpUnity.IsAssimpAvailable;
    }

    public void LoadModelButtonClicked()
    {
        Debug.Log("Ceil :" + Mathf.CeilToInt(200.0f / 128.0f));
        if(currentImportedModel != null)
        {
            DestroyImmediate(currentImportedModel);
        }

        ExtensionFilter[] extensions = new[] {
                new ExtensionFilter("3D Files", "FBX", "obj","dae","dxf","3ds" ),
            };

        //string extensions = string.Empty;

        string path = FileBrowser.OpenSingleFile("Open File", string.Empty, extensions);

        currentImportedModel = customModelImporter.LoadModel(path,matRef,normalsRecalculate.isOn,tangentsRecalculate.isOn);

        if(currentImportedModel != null)
        {
            importedModelOrignalScale = currentImportedModel.transform.localScale;
        }
    }

    private void Reset()
    {
        scaleInput.text = string.Empty;
    }

    public void OnUpdateSettingsClicked()
    {
        if(currentImportedModel != null)
        {
            currentImportedModel.transform.localScale = importedModelOrignalScale * float.Parse(scaleInput.text) ;
        }
    }
}
