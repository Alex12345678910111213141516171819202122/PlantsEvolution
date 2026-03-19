using System.Collections.Generic;
using UnityEngine;

namespace PlantsEvolution
{
    /// <summary>
    /// Универсальный рендерер для отрисовки множества однородных объектов через GPU instancing
    /// </summary>
    public class InstancedRenderer
    {
        private Mesh mesh;
        private Material material;
        private List<Matrix4x4> matrices;
        private List<Vector4> colors;
        private MaterialPropertyBlock propertyBlock;
        private static readonly int InstanceColorId = Shader.PropertyToID("_InstanceColor");
        private const int MaxInstancesPerBatch = 1023; // Unity ограничение

        public Material Material => material;
        public Mesh Mesh => mesh;

        public InstancedRenderer(Mesh mesh, Material material, int initialCapacity = 1000)
        {
            this.mesh = mesh;
            this.material = material;
            this.matrices = new List<Matrix4x4>(initialCapacity);
            this.colors = new List<Vector4>(initialCapacity);
            this.propertyBlock = new MaterialPropertyBlock();

            if (mesh == null)
            {
                Debug.LogError("[InstancedRenderer] Mesh is null!");
            }
            if (material == null)
            {
                Debug.LogError("[InstancedRenderer] Material is null!");
            }
            else if (!material.enableInstancing)
            {
                Debug.LogWarning("[InstancedRenderer] Material doesn't have GPU Instancing enabled. Enabling it now.");
                material.enableInstancing = true;
            }
        }

        /// <summary>
        /// Создаёт рендерер с примитивным мешем
        /// </summary>
        public static InstancedRenderer CreateWithPrimitive(PrimitiveType primitiveType, Material material)
        {
            GameObject temp = GameObject.CreatePrimitive(primitiveType);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(temp);
            
            return new InstancedRenderer(mesh, material);
        }

        /// <summary>
        /// Рендерит список объектов с позициями
        /// </summary>
        public void Render(IEnumerable<Vector3> positions, Quaternion rotation, Vector3 scale)
        {
            matrices.Clear();

            foreach (var position in positions)
            {
                matrices.Add(Matrix4x4.TRS(position, rotation, scale));
            }

            DrawInstances();
        }

        /// <summary>
        /// Рендерит список объектов с полными трансформациями
        /// </summary>
        public void Render(IEnumerable<Matrix4x4> transforms)
        {
            matrices.Clear();
            matrices.AddRange(transforms);
            DrawInstances();
        }

        /// <summary>
        /// Рендерит объекты с кастомной функцией для создания матриц
        /// </summary>
        public void Render<T>(IEnumerable<T> objects, System.Func<T, Matrix4x4> getMatrix)
        {
            matrices.Clear();

            foreach (var obj in objects)
            {
                matrices.Add(getMatrix(obj));
            }

            DrawInstances();
        }

        /// <summary>
        /// Рендерит объекты с отдельными позициями, вращениями и масштабами
        /// </summary>
        public void Render<T>(IEnumerable<T> objects, 
            System.Func<T, Vector3> getPosition,
            System.Func<T, Quaternion> getRotation = null,
            System.Func<T, Vector3> getScale = null)
        {
            matrices.Clear();

            foreach (var obj in objects)
            {
                Vector3 position = getPosition(obj);
                Quaternion rotation = getRotation?.Invoke(obj) ?? Quaternion.identity;
                Vector3 scale = getScale?.Invoke(obj) ?? Vector3.one;

                matrices.Add(Matrix4x4.TRS(position, rotation, scale));
            }

            DrawInstances();
        }

        /// <summary>
        /// Рендерит объекты с per-instance цветом
        /// </summary>
        public void Render<T>(IEnumerable<T> objects,
            System.Func<T, Matrix4x4> getMatrix,
            System.Func<T, Color> getColor)
        {
            matrices.Clear();
            colors.Clear();

            foreach (var obj in objects)
            {
                matrices.Add(getMatrix(obj));
                colors.Add(getColor(obj));
            }

            DrawInstancesWithColors();
        }

        private void DrawInstances()
        {
            if (mesh == null || material == null)
            {
                Debug.LogError("[InstancedRenderer] Cannot render: mesh or material is null");
                return;
            }

            // Рендерим батчами по 1023 инстанса (Unity ограничение)
            for (int i = 0; i < matrices.Count; i += MaxInstancesPerBatch)
            {
                int count = Mathf.Min(MaxInstancesPerBatch, matrices.Count - i);
                Matrix4x4[] batchMatrices = matrices.GetRange(i, count).ToArray();
                Graphics.DrawMeshInstanced(mesh, 0, material, batchMatrices, count);
            }
        }

        private void DrawInstancesWithColors()
        {
            if (mesh == null || material == null)
            {
                Debug.LogError("[InstancedRenderer] Cannot render: mesh or material is null");
                return;
            }

            if (matrices.Count != colors.Count)
            {
                Debug.LogError("[InstancedRenderer] Cannot render: matrices and colors count mismatch");
                return;
            }

            for (int i = 0; i < matrices.Count; i += MaxInstancesPerBatch)
            {
                int count = Mathf.Min(MaxInstancesPerBatch, matrices.Count - i);
                Matrix4x4[] batchMatrices = matrices.GetRange(i, count).ToArray();
                Vector4[] batchColors = colors.GetRange(i, count).ToArray();

                propertyBlock.Clear();
                propertyBlock.SetVectorArray(InstanceColorId, batchColors);

                Graphics.DrawMeshInstanced(mesh, 0, material, batchMatrices, count, propertyBlock);
            }
        }

        /// <summary>
        /// Изменить материал
        /// </summary>
        public void SetMaterial(Material newMaterial)
        {
            material = newMaterial;
            if (material != null && !material.enableInstancing)
            {
                material.enableInstancing = true;
            }
        }

        /// <summary>
        /// Изменить меш
        /// </summary>
        public void SetMesh(Mesh newMesh)
        {
            mesh = newMesh;
        }

        /// <summary>
        /// Получить количество объектов в последнем рендере
        /// </summary>
        public int GetLastInstanceCount() => matrices.Count;
    }
}
