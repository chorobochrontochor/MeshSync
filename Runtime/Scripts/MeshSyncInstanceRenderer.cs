﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.MeshSync{
    
    public class MeshSyncInstanceRenderer
    {
        private BaseMeshSync ms;
        
        public void Init(BaseMeshSync ms)
        {
            this.ms = ms;
            ms.onUpdateEntity -= OnUpdateEntity;
            ms.onUpdateEntity += OnUpdateEntity;
        }

        private Dictionary<GameObject, MeshInstanceInfo> meshInstances = new Dictionary<GameObject, MeshInstanceInfo>();

        private class MeshInstanceInfo
        {
            public Mesh Mesh;
            public List<Matrix4x4[]> Instances;
            public Material[] Materials;
        }
        
        private void OnUpdateEntity(GameObject obj, TransformData data)
        {
            
            var instances = data.FindUserProperty("instances");

            if (instances.self == IntPtr.Zero)
                return;

            if (!obj.TryGetComponent(out MeshFilter meshFilter))
            {
                Debug.LogWarningFormat("[MeshSync] Object {0} has instances info but no MeshFilter", obj.name);
                return;
            }

            if (!obj.TryGetComponent(out MeshRenderer renderer))
            {
                Debug.LogWarningFormat("[MeshSync] Object {0} has instances info but no MeshRenderer", obj.name);
            }
            
            var mesh = meshFilter.sharedMesh;

            
            if (!meshInstances.TryGetValue(obj, out MeshInstanceInfo entry))
            {
                entry = new MeshInstanceInfo
                {
                    Mesh = mesh
                };
                
                meshInstances.Add(obj, entry);
            }

            entry.Instances = DivideArrays(instances.matrixArray);
            entry.Materials = renderer.sharedMaterials;
            foreach (var mat in entry.Materials)
            {
                mat.enableInstancing = true;
            }            
        }

        private List<Matrix4x4[]> DivideArrays(Matrix4x4[] arrays)
        {
            var result = new List<Matrix4x4[]>();
            var iterations = arrays.Length / 1023;
            for (var i = 0; i < iterations; i++)
            {
                var array = new Matrix4x4[1023];
                
                Array.Copy(
                    arrays, 
                    i * 1023, 
                    array, 
                    0, 
                    1023);
                
                result.Add(array);
            }

            var remainder = arrays.Length % 1023;
            if (remainder > 0)
            {
                var array = new Matrix4x4[remainder];
                
                Array.Copy(
                    arrays, 
                    iterations*1023, 
                    array, 
                    0, 
                    remainder);
                
                result.Add(array);
            }

            return result;
        }
        
        public void Draw()
        {
            foreach (var entry in meshInstances)
            {
                RenderInstances(entry.Value);
            }
        }

        private void RenderInstances(MeshInstanceInfo entry)
        {
            var mesh = entry.Mesh;
            var matrixBatches = entry.Instances;

            if (entry.Materials.Length == 0)
                return;
            
            for (var i = 0; i < mesh.subMeshCount; i++)
            {
                // Try to get the material in the same index position as the mesh
                // or the last material.
                var materialIndex = Mathf.Clamp(i, 0, entry.Materials.Length -1);
                
                var material = entry.Materials[materialIndex];
                for (var j = 0; j < matrixBatches.Count; j++)
                {
                    var batch = matrixBatches[j];
                    Graphics.DrawMeshInstanced(mesh, i, material, batch);
                }
            }
        }
    }
}

