using System.Collections.Generic;
using UnityEngine;

namespace PlantsEvolution
{
    /// <summary>
    /// Рендерит клетки деревьев с группировкой по TreeID и автоматическим батчингом
    /// </summary>
    public class TreeCellRenderer
    {
        private Mesh mesh;
        private Material baseMaterial;
        private Material instanceMaterial;
        private MaterialPropertyBlock propertyBlock;
        private List<Matrix4x4> matrixBuffer;
        
        private const int MaxInstancesPerBatch = 1023;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public TreeCellRenderer(Mesh mesh, Material material, int initialCapacity = 10000)
        {
            this.mesh = mesh;
            this.baseMaterial = material;
            this.instanceMaterial = new Material(material); // Создаём рабочую копию
            this.propertyBlock = new MaterialPropertyBlock();
            this.matrixBuffer = new List<Matrix4x4>(initialCapacity);

            if (mesh == null)
            {
                Debug.LogError("[TreeCellRenderer] Mesh is null!");
            }
            if (baseMaterial == null)
            {
                Debug.LogError("[TreeCellRenderer] Material is null!");
            }
            else if (!instanceMaterial.enableInstancing)
            {
                Debug.LogWarning("[TreeCellRenderer] Enabling GPU Instancing on material.");
                instanceMaterial.enableInstancing = true;
            }
        }

        /// <summary>
        /// Создаёт рендерер с примитивным мешем
        /// </summary>
        public static TreeCellRenderer CreateWithPrimitive(PrimitiveType primitiveType, Material material)
        {
            GameObject temp = GameObject.CreatePrimitive(primitiveType);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(temp);
            
            return new TreeCellRenderer(mesh, material);
        }

        /// <summary>
        /// Рендерит все клетки всех деревьев с группировкой по дереву
        /// </summary>
        public void RenderTrees(List<TreeElement> trees, Quaternion rotation, Vector3 scale)
        {
            if (mesh == null || instanceMaterial == null)
            {
                Debug.LogError("[TreeCellRenderer] Cannot render: mesh or material is null");
                return;
            }

            foreach (var tree in trees)
            {
                RenderTree(tree, rotation, scale);
            }
        }

        /// <summary>
        /// Рендерит клетки одного дерева (с автоматическим разбиением на батчи)
        /// </summary>
        public void RenderTree(TreeElement tree, Quaternion rotation, Vector3 scale)
        {
            if (tree.GroupPositions == null || tree.GroupPositions.Count == 0)
                return;

            // Подготавливаем матрицы
            matrixBuffer.Clear();
            foreach (var position in tree.GroupPositions)
            {
                matrixBuffer.Add(Matrix4x4.TRS(position, rotation, scale));
            }

            // Устанавливаем цвет дерева через MaterialPropertyBlock
            propertyBlock.SetColor(BaseColorId, tree.Color);
            
            // Рендерим батчами
            int cellCount = matrixBuffer.Count;
            for (int i = 0; i < cellCount; i += MaxInstancesPerBatch)
            {
                int batchSize = Mathf.Min(MaxInstancesPerBatch, cellCount - i);
                Matrix4x4[] batchMatrices = new Matrix4x4[batchSize];
                
                for (int j = 0; j < batchSize; j++)
                {
                    batchMatrices[j] = matrixBuffer[i + j];
                }
                
                Graphics.DrawMeshInstanced(mesh, 0, instanceMaterial, batchMatrices, batchSize, propertyBlock);
            }
        }

        /// <summary>
        /// Рендерит клетки с кастомной трансформацией для каждой
        /// </summary>
        public void RenderTree(TreeElement tree, System.Func<Vector3, Matrix4x4> getMatrix)
        {
            if (tree.GroupPositions == null || tree.GroupPositions.Count == 0)
                return;

            matrixBuffer.Clear();
            foreach (var position in tree.GroupPositions)
            {
                matrixBuffer.Add(getMatrix(position));
            }

            propertyBlock.SetColor(BaseColorId, tree.Color);
            int cellCount = matrixBuffer.Count;
            
            for (int i = 0; i < cellCount; i += MaxInstancesPerBatch)
            {
                int batchSize = Mathf.Min(MaxInstancesPerBatch, cellCount - i);
                Matrix4x4[] batchMatrices = new Matrix4x4[batchSize];
                
                for (int j = 0; j < batchSize; j++)
                {
                    batchMatrices[j] = matrixBuffer[i + j];
                }
                
                Graphics.DrawMeshInstanced(mesh, 0, instanceMaterial, batchMatrices, batchSize, propertyBlock);
            }
        }

        /// <summary>
        /// Обновить материал
        /// </summary>
        public void SetMaterial(Material newMaterial)
        {
            baseMaterial = newMaterial;
            if (instanceMaterial != null)
            {
                Object.Destroy(instanceMaterial);
            }
            instanceMaterial = new Material(newMaterial);
            if (instanceMaterial != null && !instanceMaterial.enableInstancing)
            {
                instanceMaterial.enableInstancing = true;
            }
        }

        /// <summary>
        /// Обновить меш
        /// </summary>
        public void SetMesh(Mesh newMesh)
        {
            mesh = newMesh;
        }

        /// <summary>
        /// Получить общее количество клеток в последнем рендере
        /// </summary>
        public int GetLastCellCount() => matrixBuffer.Count;
    }
}
