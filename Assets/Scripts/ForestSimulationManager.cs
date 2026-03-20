using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlantsEvolution
{
public class ForestSimulationManager : MonoBehaviour
{
    [SerializeField] 
    bool DebugMode = true;
    [SerializeField]
    bool HighlightTopCells = false;
    [SerializeField]
    int initialTreesCount = 5;
    private CellGrid _cellGrid;
    private List<TreeElement> Trees = new List<TreeElement>();
    private PlantingProcess _plantingProcess;
    private GrowthProcess _growthProcess;
    private bool photosynthesisProcessed = false;

    private TreesEvolutionProcess _evolutionProcess;

    private CommonTreeBuilder treeBuilder;
    private PhotosyntesisProcess _photosyntesisProcess;
    private InstancedRenderer _instancedRenderer;
    private void Init()
    {
        _cellGrid = new CellGrid(new Vector3Int(0, 0, 0), 2.0f, 10, 10, 10.0f, 5);

        Trees = new List<TreeElement>();

        _growthProcess = new GrowthProcess(_cellGrid.cellSize);

        _plantingProcess = new PlantingProcess(_growthProcess, _cellGrid);

        _photosyntesisProcess = new PhotosyntesisProcess();

        _evolutionProcess = new TreesEvolutionProcess(mutationRate: 1, evolutionRate: 2);

        treeBuilder = new CommonTreeBuilder();

        Shader instancedColorShader = Shader.Find("PlantsEvolution/InstancedColorURP");
        if (instancedColorShader == null)
        {
            Debug.LogWarning("[Simulation] Instanced color shader not found. Falling back to URP/Lit.", this);
            instancedColorShader = Shader.Find("Universal Render Pipeline/Lit");
        }

        Material cellMaterial = new Material(instancedColorShader);
        cellMaterial.enableInstancing = true;
        cellMaterial.color = Color.white;
        _instancedRenderer = InstancedRenderer.CreateWithPrimitive(PrimitiveType.Cube, cellMaterial);
    }
    private Coroutine _simulateRoutine;
private Coroutine _plantRoutine;


private IEnumerator SimulateLoop()
{
    var wait = new WaitForSeconds(0.05f);
    while (true) { Simulate(); yield return wait; }
}

private IEnumerator PlantGenerationLoop()
{
    var wait = new WaitForSeconds(5);
    while (true) { PlantNewGeneration(); yield return wait; }
}

private void OnDisable()
{
    if (_simulateRoutine != null) StopCoroutine(_simulateRoutine);
    if (_plantRoutine != null) StopCoroutine(_plantRoutine);
}

    public void Start()
    {
        Init();
        if(!DebugMode)
        {
        _simulateRoutine = StartCoroutine(SimulateLoop());
        _plantRoutine = StartCoroutine(PlantGenerationLoop());
        }
        else
        {
            Debug.LogWarning("[Simulation] Running in debug mode. Automatic simulation and planting are disabled.", this);
        }
        Debug.unityLogger.logEnabled = true;

        Trees = _plantingProcess.PlantInitialTrees(treeBuilder, initialTreesCount*initialTreesCount);

        Debug.Log($"[Simulation] Simulation initialized with {Trees.Count} tree(s)", this);
    }
    public void Update()
    {
        if (_instancedRenderer == null || _cellGrid == null) return;
        // ----------------------------------------------------------------------------------------
        Dictionary<int, Color> treeColorsById = BuildTreeColorLookup();
        HashSet<Vector3> topCellPositions = HighlightTopCells ? _cellGrid.GetTopCellPositions() : null;
        _instancedRenderer.Render(
            _cellGrid.Cells,
            cellEntry => Matrix4x4.TRS(cellEntry.Key, Quaternion.identity, Vector3.one),
            cellEntry => (topCellPositions != null && topCellPositions.Contains(cellEntry.Key))
                ? Color.white
                : (treeColorsById.TryGetValue(cellEntry.Value.CellID, out Color color) ? color : Color.white)
        );
        // ----------------------------------------------------------------------------------------
        if (DebugMode)
        {
            HandleDebugInput();
        }
    }
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {   
            TreeElement tree = _plantingProcess.PlantTree(treeBuilder, Trees.Count, _cellGrid.Position + new Vector3Int(Trees.Count * 2, 0, 0));
            Trees.Add(tree);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _growthProcess.Process(Trees, _cellGrid);
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            photosynthesisProcessed = !photosynthesisProcessed;
        }
        if(photosynthesisProcessed)
        {
            _photosyntesisProcess.Process(Trees, _cellGrid.topCellId);
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            ClearSimulation();
        }
        if(Input.GetKeyDown(KeyCode.N))
        {
            PlantNewGeneration();   
        }
    }

    private void Simulate()
    {
        _growthProcess.Process(Trees, _cellGrid);
        _photosyntesisProcess.Process(Trees, _cellGrid.topCellId);
    }
    private void ClearSimulation()
    {
        foreach (var tree in Trees)
        {
            tree.Destroy(_cellGrid);
        }
        Trees.Clear();
        _growthProcess.ClearGrowthPositions();
    }

    private void PlantNewGeneration()
    {
        List<TreeElement> evolvedTrees = _evolutionProcess.Process(Trees, initialTreesCount);
        if (evolvedTrees == null || evolvedTrees.Count == 0)
        {
            Debug.LogWarning("[PlantNewGeneration] No evolved trees to plant.");
            return;
        }

        ClearSimulation();

        _plantingProcess.PlantNewGeneration(evolvedTrees);
        Trees = new List<TreeElement>(evolvedTrees);

        Debug.Log($"[PlantNewGeneration] Planted new generation with {evolvedTrees.Count} tree(s).", this);
    }
        

    private Dictionary<int, Color> BuildTreeColorLookup()
    {
        Dictionary<int, Color> treeColorsById = new Dictionary<int, Color>(Trees.Count);

        foreach (TreeElement tree in Trees)
        {
            treeColorsById[tree.TreeID] = tree.Color;
        }

        return treeColorsById;
    }
}
}
