# AssImp Unity Wrapper

I was trying to find custom asset import on runtime. Currently in unity there is no direct way to do this. I searched on the web I could only find one plugin which is AssImp (https://github.com/assimp/assimp) which is written in C++. After that tried to find .Net wrapper for this and found out that there is AssImp official updated plugin on bitbucket(https://bitbucket.org/Starnick/assimpnet). In this plugin there are also an script which loads the dlls in unity which is also used in the project.

After that tried to find if someone has implemented this in unity. There are multiple people who tried implemented but could not find any thing which is complete. So decided to write my own wrapper to handle the AssImp .Net dlls in unity. 

1.First step is to compile the latest AssImp .Net source code and get the required Dlls. Add This project already contains working dlls and they can be replaced with new ones too. 

2.Next is use CrossTales File Browser by which we can get the path of the asset.

3. Then we have to intialize AssImp with the following settings 

        importer.SetConfig(new Assimp.Configs.MeshVertexLimitConfig(60000));
        importer.SetConfig(new Assimp.Configs.MeshTriangleLimitConfig(60000));
        importer.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
        importer.SetConfig(new Assimp.Configs.SortByPrimitiveTypeConfig(Assimp.PrimitiveType.Line | Assimp.PrimitiveType.Point));

        postProcessSteps = Assimp.PostProcessPreset.TargetRealTimeMaximumQuality | Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipWindingOrder;

4. We can always use the property Assimp.AssimpUnity.IsAssimpAvailable to check if AssImp is available on platform.

5. Now we are good to go to import the asset using Scene scene = importer.ImportFile(modelPath, postProcessSteps).

6. In scene we will get al the data for the asset.

7. We have to display the data in unity. This code is present in CustomAssetImporter.

This project also uses Free file browser plugin created by CrossTales (https://assetstore.unity.com/packages/tools/gui/file-browser-windows-macos-98716)
