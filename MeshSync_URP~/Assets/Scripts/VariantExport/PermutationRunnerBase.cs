﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.MeshSync.VariantExport
{
    abstract class PermutationRunnerBase
    {
        enum SaveMode
        {
            DryRunWithoutSyncAndSave,
            SyncButDoNotSave,
            SaveAsPrefab
        }

        protected MeshSyncServer server => variantExporter.Server;

        protected VariantExporter variantExporter;

        protected object[] currentSettings;

        public int counter;

        SaveMode saveMode = SaveMode.SaveAsPrefab;

        public ReadOnlyCollection<PropertyInfoDataWrapper> propertyInfos => variantExporter.EnabledProperties;


        public abstract int VariantCount
        {
            get;
        }

        public int TotalPermutationCount
        {
            get
            {
                int total = 1;

                foreach (var prop in server.propertyInfos)
                {
                    int range = 1;

                    // TODO: Make this work with float ranges:
                    switch (prop.type)
                    {
                        case PropertyInfoData.Type.Int:
                            range = (int)(prop.max - prop.min) + 1;
                            break;
                    }

                    total *= range;
                }

                return total;
            }
        }

        public PermutationRunnerBase(VariantExporter variantExporter)
        {
            this.variantExporter = variantExporter;
        }

        protected abstract IEnumerator ExecutePermutations(int propIdx);

        protected IEnumerator Save()
        {
            var permutationCount = VariantCount;
            if (EditorUtility.DisplayCancelableProgressBar("Exporting", $"Variant {counter + 1}/{permutationCount}", (float)counter / permutationCount))
            {
                variantExporter.StopExport();
                yield break;
            }

            if (saveMode != SaveMode.DryRunWithoutSyncAndSave)
            {
                // Set all properties here at once so they can be sent to blender together:
                for (int i = 0; i < propertyInfos.Count; i++)
                {
                    var prop = propertyInfos[i];
                    prop.NewValue = currentSettings[i];
                }

                var s = new StringBuilder();
                foreach (var prop in propertyInfos)
                {
                    s.AppendLine(prop.ToString());
                }

                Debug.Log(s);

                yield return new WaitForMeshSync(server);

                switch (saveMode)
                {
                    case SaveMode.SaveAsPrefab:
                        yield return SavePrefab();
                        break;

                    case SaveMode.SyncButDoNotSave:
                        break;

                    default:
                        throw new NotImplementedException($"save mode {saveMode} not implemented");
                }
            }

            counter++;

            if (counter >= permutationCount)
            {
                Debug.Log($"Finished exporting {variantExporter.SaveFile}.");

                variantExporter.StopExport();
            }

            // test:
            //variantExporter.StopExport();

            yield break;
        }

        IEnumerator SavePrefab()
        {
            var outputFilePath = variantExporter.SavePath;

            if (!Directory.Exists(outputFilePath))
            {
                Directory.CreateDirectory(outputFilePath);
            }

            // test:
            //var commonAssetStorePath = Path.Combine(outputFilePath, $"{variantExporter.SaveFile}_{counter}_assets");
            //variantExporter.Server.SetAssetsFolder(commonAssetStorePath);

            variantExporter.Server.ExportMeshes();

            var prefabSave = UnityEngine.Object.Instantiate(server.gameObject);

            prefabSave.GetComponent<MeshSyncServer>().enabled = false;

            outputFilePath = Path.Combine(outputFilePath, $"{variantExporter.SaveFile}_{counter}.prefab");
            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabSave, outputFilePath);

            UnityEngine.Object.DestroyImmediate(prefabSave);

            yield break;
        }

        public IEnumerator Start()
        {
            if (EditorUtility.DisplayCancelableProgressBar("Exporting", $"Variant 1/{VariantCount}", 0))
            {
                variantExporter.StopExport();
                yield break;
            }

            currentSettings = new object[variantExporter.enabledProperties.Count];

            var outputFilePath = variantExporter.SavePath;

            if (Directory.Exists(outputFilePath))
            {
                foreach (var prefabFile in Directory.EnumerateFiles(outputFilePath, "*.prefab"))
                {
                    AssetDatabase.DeleteAsset(prefabFile);
                    //File.Delete(prefabFile);
                }

                AssetDatabase.Refresh();
            }

            var commonAssetStorePath = Path.Combine(outputFilePath, $"{variantExporter.SaveFile}_assets");

            variantExporter.Server.SetAssetsFolder(commonAssetStorePath);
            variantExporter.Server.ExportMeshes();
            variantExporter.Server.ExportMaterials();

            yield return Next(-1);
        }

        protected IEnumerator Next(int propIdx)
        {
            propIdx++;

            if (propIdx == propertyInfos.Count)
            {
                yield return Save();
            }
            else
            {
                yield return ExecutePermutations(propIdx);
            }
        }
    }
}
