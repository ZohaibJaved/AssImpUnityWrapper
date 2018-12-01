using Assimp;
using Assimp.Configs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomAssetImporter
{
    Scene ImportModel(string modelPath)
    {
        if (File.Exists(modelPath))
        {
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            importer.SetConfig(new Assimp.Configs.MeshVertexLimitConfig(60000));
            importer.SetConfig(new Assimp.Configs.MeshTriangleLimitConfig(60000));
            importer.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
            importer.SetConfig(new Assimp.Configs.SortByPrimitiveTypeConfig(Assimp.PrimitiveType.Line | Assimp.PrimitiveType.Point));

            Assimp.PostProcessSteps postProcessSteps = Assimp.PostProcessPreset.TargetRealTimeMaximumQuality | Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipWindingOrder;

            Scene scene = importer.ImportFile(modelPath, postProcessSteps);

            if (scene != null)
            {
                Debug.Log("Model Successfully Loaded");

                return scene;
            }
        }
        else
        {
            Debug.Log("ImportModel : File not available at path : " + modelPath);
        }

        return null;
    }
    public GameObject LoadModel(string modelPath, UnityEngine.Material mat, bool shouldCalculateNormals,bool shouldCalculateTangents)
    {
        Scene assImpScene = ImportModel(modelPath);

        List<UnityEngine.Mesh> meshes = new List<UnityEngine.Mesh>();

        // model contains multiple meshes

        if(assImpScene != null && assImpScene.HasMeshes)
        {
            foreach(var assImpMesh in assImpScene.Meshes)
            {
                UnityEngine.Mesh mesh = new UnityEngine.Mesh();
                
                // Verticecs - Uvs and Tri's should be in this specific order to work. 
                // Vertices
                mesh.vertices = assImpMesh.Vertices != null ? assImpMesh.Vertices.ConvertAll(item => new Vector3(item.X, item.Y, item.Z)).ToArray() : null;

                // uvs
                mesh.uv = assImpMesh.HasTextureCoords(0) && assImpMesh.TextureCoordinateChannels[0] != null ? assImpMesh.TextureCoordinateChannels[0].ConvertAll(item => new Vector2(item.X, item.Y)).ToArray() : null;

                // Triangles
                mesh.triangles = assImpMesh.GetIndices();

                // normals
                mesh.normals = assImpMesh.HasNormals && assImpMesh.Normals != null ? assImpMesh.Normals.ConvertAll(item => new Vector3(item.X, item.Y, item.Y)).ToArray() : null;

                mesh.tangents = assImpMesh.Tangents != null ? assImpMesh.Tangents.ConvertAll(item => new Vector4(item.X, item.Y, item.Z, 1)).ToArray() : null;

                bool noNormalsData = mesh.normals == null || mesh.normals.Length == 0;
                bool noTangensData = mesh.tangents == null || mesh.tangents.Length == 0;
                if ( noNormalsData || shouldCalculateNormals)
                {
                    if(noNormalsData)
                    {
                        Debug.Log("Mesh Normals are null");
                    }
                    
                    mesh.RecalculateNormals();
                    //mesh.RecalculateNormals(60);
                }

                if( noTangensData || shouldCalculateTangents)
                {
                    if(noTangensData)
                    {
                        Debug.Log("Mesh Normals are null");
                    }
                    
                    mesh.RecalculateTangents();
                    //mesh.CustomRecalculateTangents();
                }

                mesh.name = assImpMesh.Name;

                Debug.Log("Mesh Name : " + assImpMesh.Name);
                
                meshes.Add(mesh);
            }

            Debug.Log("Meshes Imported in Unity : " + meshes.Count);

            return ProcessNodes(assImpScene.RootNode,meshes,mat);
        }

        return null;

    }

    public GameObject ProcessNodes(Assimp.Node rootNode,List<UnityEngine.Mesh> meshesData,UnityEngine.Material mat)
    {
        GameObject unityRoot = new GameObject(rootNode.Name);
        if (rootNode.HasChildren)
        {
            foreach (Assimp.Node childNode in rootNode.Children)
            {
                GameObject unityChild = ProcessNodes(childNode,meshesData,mat);
                unityChild.transform.parent = unityRoot.transform;
            }
        }

        Debug.Log("Node Name : " + rootNode.Name);

        Assimp.Vector3D position, scaling;
        Assimp.Quaternion rotation;
        rootNode.Transform.Decompose(out scaling, out rotation, out position);
        unityRoot.transform.localPosition = new Vector3(position.X, position.Y, position.Z);
        unityRoot.transform.localRotation = new UnityEngine.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        unityRoot.transform.localScale = new Vector3(scaling.X, scaling.Y, scaling.Z);

        // Apply Mesh

        List<CombineInstance> combinedInstances = new List<CombineInstance>();

        bool containsMultipleMeshes = rootNode.MeshIndices.Count > 1; 

        foreach (var meshIndex in rootNode.MeshIndices)
        {
            var foundMesh = meshesData[meshIndex];

            if (foundMesh != null)
            {
                if(containsMultipleMeshes)// If per GO contains multiple meshes then go through each mesh and generate GO.
                {
                    GameObject meshGO = new GameObject();

                    meshGO.transform.parent = unityRoot.transform;

                    AddMeshGO(foundMesh, meshGO, unityRoot.name,mat);
                }
                else
                {
                    AddMeshGO(foundMesh, unityRoot, unityRoot.name,mat);
                }
            }
        }
        
        // Mesh Combine Code

        //foreach (var meshIndex in rootNode.MeshIndices)
        //{
        //    var foundMesh = meshesData[meshIndex];

        //    if (foundMesh != null)
        //    {
        //        CombineInstance newMesh = new CombineInstance();
        //        newMesh.mesh = foundMesh;
        //        newMesh.transform = unityRoot.transform.localToWorldMatrix;

        //        combinedInstances.Add(newMesh);
        //    }

        //}

        //if (combinedInstances.Count > 1) 
        //{
        //    unityRoot.AddComponent<MeshFilter>().mesh.CombineMeshes(combinedInstances.ToArray());
        //    unityRoot.AddComponent<MeshRenderer>().sharedMaterial = matRef;
        //}
        //else if(combinedInstances.Count == 1) // if per GO only one mesh.
        //{
        //    unityRoot.AddComponent<MeshFilter>().mesh = combinedInstances[0].mesh;
        //    unityRoot.AddComponent<MeshRenderer>().sharedMaterial = matRef;
        //}

        return unityRoot;
    }

    GameObject AddMeshGO(UnityEngine.Mesh mesh,GameObject meshGO,string goName, UnityEngine.Material mat)
    {
        meshGO.AddComponent<MeshFilter>().mesh = mesh;
        meshGO.AddComponent<MeshRenderer>().sharedMaterial = mat;

        meshGO.name = goName;

        return meshGO;
    }

}
