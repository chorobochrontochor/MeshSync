using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.MeshSync.Editor
{
    [CustomEditor(typeof(MeshSyncServer))]
    internal class MeshSyncServerInspector : BaseMeshSyncInspector
    {


        //----------------------------------------------------------------------------------------------------------------------

        public void OnEnable()
        {
            m_meshSyncServer = target as MeshSyncServer;
        }

        //----------------------------------------------------------------------------------------------------------------------
        public override void OnInspectorGUI()
        {
            Undo.RecordObject(m_meshSyncServer, "MeshSyncServer Update");

            EditorGUILayout.Space();
            DrawServerSettings(m_meshSyncServer);
            DrawAssetSyncSettings(m_meshSyncServer);
            DrawImportSettings(m_meshSyncServer);
            DrawMiscSettings(m_meshSyncServer);
            DrawDefaultMaterialList(m_meshSyncServer);
            DrawAnimationTweak(m_meshSyncServer);
            DrawExportAssets(m_meshSyncServer);
            DrawSliders(m_meshSyncServer);

            if (m_meshSyncServer.m_DCCAsset != null && m_meshSyncServer.m_DCCInterop == null)
            {
                m_meshSyncServer.m_DCCInterop = GetLauncherForAsset(m_meshSyncServer.m_DCCAsset);
            }

            m_meshSyncServer.m_DCCInterop?.DrawDCCToolVersion(m_meshSyncServer);

            DrawPluginVersion();
        }

        //----------------------------------------------------------------------------------------------------------------------

        public void DrawServerSettings(MeshSyncServer t)
        {
            var styleFold = EditorStyles.foldout;
            styleFold.fontStyle = FontStyle.Bold;

            bool isServerStarted = m_meshSyncServer.IsServerStarted();
            string serverStatus = isServerStarted ? "Server (Status: Started)" : "Server (Status: Stopped)";
            t.foldServerSettings = EditorGUILayout.Foldout(t.foldServerSettings, serverStatus, true, styleFold);
            if (t.foldServerSettings)
            {

                bool autoStart = EditorGUILayout.Toggle("Auto Start", m_meshSyncServer.IsAutoStart());
                m_meshSyncServer.SetAutoStartServer(autoStart);

                //Draw GUI that are disabled when autoStart is true
                EditorGUI.BeginDisabledGroup(autoStart);
                int serverPort = EditorGUILayout.IntField("Server Port:", (int)m_meshSyncServer.GetServerPort());
                m_meshSyncServer.SetServerPort((ushort)serverPort);
                GUILayout.BeginHorizontal();
                if (isServerStarted)
                {
                    if (GUILayout.Button("Stop", GUILayout.Width(110.0f)))
                    {
                        m_meshSyncServer.StopServer();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start", GUILayout.Width(110.0f)))
                    {
                        m_meshSyncServer.StartServer();
                    }

                }
                GUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();

                string prevFolder = t.GetAssetsFolder();
                string selectedFolder = AssetEditorUtility.NormalizePath(
                    EditorGUIDrawerUtility.DrawFolderSelectorGUI("Asset Dir", "Asset Dir", prevFolder, null)
                );
                if (selectedFolder != prevFolder)
                {
                    if (string.IsNullOrEmpty(selectedFolder) || !AssetEditorUtility.IsPathNormalized(selectedFolder))
                    {
                        Debug.LogError($"[MeshSync] {selectedFolder} is not under Assets. Ignoring.");
                    }
                    else
                    {
                        t.SetAssetsFolder(selectedFolder);
                    }

                }

                Transform rootObject = (Transform)EditorGUILayout.ObjectField("Root Object", t.GetRootObject(),
                    typeof(Transform), allowSceneObjects: true);
                t.SetRootObject(rootObject);

                EditorGUILayout.Space();
            }
        }

        void DrawSliders(BaseMeshSync player)
        {
            var records = player.modifiersInfo;

            foreach (var entry in records)
            {
                var gameObject = entry.Key;
                var name = gameObject.name;

                EditorGUILayout.LabelField(name);

                EditorGUI.BeginChangeCheck();

                var modifiers = entry.Value;
                foreach (var modifier in modifiers)
                {
                    var modifierType = modifier.Type;
                    switch (modifierType)
                    {
                        case BaseMeshSync.ModifierInfo.ModifierType.Float:
                            var floatModifier = modifier as BaseMeshSync.FloatModifierInfo;
                            floatModifier.Value = EditorGUILayout.Slider(floatModifier.Name, floatModifier.Value, floatModifier.Min, floatModifier.Max);
                            break;
                        case BaseMeshSync.ModifierInfo.ModifierType.Int:
                            var intModifier = modifier as BaseMeshSync.IntModifierInfo;
                            intModifier.Value = EditorGUILayout.IntSlider(intModifier.Name, intModifier.Value, intModifier.Min, intModifier.Max);
                            break;
                        case BaseMeshSync.ModifierInfo.ModifierType.Vector:
                            var vectorModifier = modifier as BaseMeshSync.VectorModifierInfo;
                            vectorModifier.Value = EditorGUILayout.Vector3Field(vectorModifier.Name, vectorModifier.Value);
                            break;
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    // TODO: Send modifiers back to blender here.
                }
            }
        }

        public static IDCCLauncher GetLauncherForAsset(GameObject asset)
        {
            // TODO: Check asset path here and choose IDCCLauncher implementation for the given type.
            // var assetPath = AssetDatabase.GetAssetPath(asset).Replace("Assets/", string.Empty);
            return new BlenderLauncher();
        }

        //----------------------------------------------------------------------------------------------------------------------                
        private MeshSyncServer m_meshSyncServer = null;
    }

} //end namespace
