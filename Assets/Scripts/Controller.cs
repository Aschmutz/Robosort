using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Json.Schema;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum RotationType{
    Random,
    Initial,
    Custom,
    Computed //TODO Implement Computed Rotation (Nice to have)
}

 internal struct RenderInformation
{
    
    public string modelPath;
    public float simulationSpeed;
    public float simulationTime;
    public int amount;
    public RotationType rotationType;
    public Quaternion rotation;
    
    public RenderInformation(string modelPath, float simulationSpeed, float simulationTime, int amount, RotationType rotationType, Quaternion rotation)
    {
        this.modelPath = modelPath;
        this.simulationSpeed = simulationSpeed;
        this.simulationTime = simulationTime;
        this.amount = amount;
        this.rotationType = rotationType;
        this.rotation = rotation;
    }
}
public class Controller : MonoBehaviour
{
    [SerializeField] private IModelLoader loader;

    [SerializeField] private Rendering renderComponent;
    [SerializeField] private Transform renderTransform;
    [SerializeField] private string parameterPath; //A file containing the Parameters for loading and rendering the objects in *A* format
    [SerializeField] private LayerMask environmentLayerMask;
    private string outputPath;
    private RenderInformation[] modelsToRender;
    void Start()
    {
        Console.WriteLine("Reading the Input Files");
        modelsToRender = ParametersToRenderInformationArray(parameterPath);
        Console.WriteLine("Starting the Render");
        StartCoroutine(RenderAllModels());

    }

    IEnumerator RenderAllModels()
    {
        Rigidbody rb = null;
        string oldPath = null;
        yield return null; // Waits so that the Start method of other components can be called first
        GameObject loaded = null;
        int operationId = 0;
        foreach (RenderInformation information in modelsToRender)
        {
            if (information.modelPath != oldPath)
            {
                
                if(loaded != null) Destroy(loaded);
                yield return null;
                
                loaded = loader.Load(information.modelPath);
                if (loaded == null)
                {
                    Debug.Log("Skipping " + information.modelPath);
                    Console.WriteLine("Skipping " + information.modelPath);
                    continue;
                }
                
                oldPath = information.modelPath;
                rb = loaded.GetComponent<Rigidbody>();

                operationId = 0;
            }
            
            Time.timeScale = information.simulationSpeed;
            
            for (int i = 0; i < information.amount; i++)
            {
                loaded.transform.position = renderTransform.position;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.mass = 2;
                switch (information.rotationType)
                {
                    case RotationType.Initial: 
                        loaded.transform.rotation = renderTransform.rotation;
                        break;
                    case RotationType.Random:
                        loaded.transform.rotation = Random.rotationUniform;
                        break;
                    case RotationType.Custom:
                        loaded.transform.rotation = information.rotation;
                        break;
                    case RotationType.Computed:
                        Debug.Log("Unsupported Rotation Type! Falling back to Initial!");
                        loaded.transform.rotation = renderTransform.rotation;
                        break;
                    default:
                        Debug.Log("Unknown Rotation Type! Falling back to Initial!");
                        loaded.transform.rotation = renderTransform.rotation;
                        break;
                }

                if (information.simulationTime < 0)
                {
                    loaded.transform.position -= new Vector3(0,CalculateFloorDistance(loaded.GetComponent<MeshFilter>()),0);
                }

                loaded.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0, 1, 0, 1, 0, 1, 1, 1);
                
                string modelName = information.modelPath[(information.modelPath.LastIndexOf(Path.DirectorySeparatorChar) + 1)..information.modelPath.LastIndexOf('.')];
                string prefix = "m-"+modelName+"_op-"+operationId+"_id-"+i+"_img-";
                yield return StartCoroutine(renderComponent.RenderSingleImage(outputPath,information.simulationTime,prefix));
            }

            operationId++;
        }
        Application.Quit();
    }

    private float CalculateFloorDistance(MeshFilter model)
    {
        Vector3 lowestPoint = Vector3.positiveInfinity;
        foreach (Vector3 vertex in model.mesh.vertices)
        {
            Vector3 transformedVertex = model.transform.TransformPoint(vertex);
            if(transformedVertex.y < lowestPoint.y) lowestPoint = transformedVertex;
        }
        
        if (!Physics.Raycast(lowestPoint, Vector3.down, out RaycastHit hit,100,environmentLayerMask.value))
            return 0;
        return hit.distance;
    }

    /**
     * Returns a list of RenderInformation parsed from a JSON file located at PATH. The information in the file must be in compliance with RenderInformationSchema.json
     *
     * Usefull sites:
     * https://docs.json-everything.net/
     * https://www.educative.io/answers/how-to-read-a-json-file-in-c-sharp
     */
    private RenderInformation[] ParametersToRenderInformationArray(string path)
    {
        string parametersJsonText = File.ReadAllText(path);
        JsonDocument parametersJson = JsonDocument.Parse(parametersJsonText);

        //Validation Start
        
        //Loading the Schemas from the Resources folder, to allow for loading at Runtime in Standalone application.
        string renderInfoSchemaJson = Resources.Load<TextAsset>("Schemas/RenderInfoBundle").ToString();
        JsonSchema renderInfoSchema = JsonSchema.FromText(renderInfoSchemaJson);
        
        EvaluationOptions options = new EvaluationOptions();
        options.OutputFormat = OutputFormat.Flag;
        EvaluationResults results = renderInfoSchema.Evaluate(parametersJson,options);
        if (!results.IsValid) // If the result is invalid it gets reanalyzed with Hieachical Error output. This is done because Flag is faster.
        {
            options.OutputFormat = OutputFormat.Hierarchical;
            results = renderInfoSchema.Evaluate(parametersJson,options);
            string errorText = "ERROR:";
            if (results.HasErrors)
            {
                foreach (KeyValuePair<string, string> error in results.Errors)
                {
                    errorText += error.Value + "\n";
                }
            }
            foreach (EvaluationResults resultsDetail in results.Details) //Goes through all nested results because the error is also nested
            {
                if (resultsDetail.HasErrors)
                {
                    foreach (KeyValuePair<string, string> error in resultsDetail.Errors)
                    {
                        errorText += error.Value + "\n";
                    }
                }
            }
            /*Debug.Log(parametersJsonText);
            Debug.Log(renderInfoSchemaJson);*/
            Debug.Log(errorText);
            Console.WriteLine(errorText);
            Application.Quit();
            return Array.Empty<RenderInformation>();
        }

        //Validation End
        
        RenderInformationJson renderInformationJson = parametersJson.Deserialize<RenderInformationJson>();
        
        
        List<RenderInformation> renderInformation = new List<RenderInformation>();
        this.outputPath = renderInformationJson.outputFolder;
        string[] models = Directory.GetFiles(Directory.GetCurrentDirectory()+Path.DirectorySeparatorChar + renderInformationJson.inputFolder,"*.obj"); //Gets the modesls full with their full paths
        foreach(string model in models)
        {
            foreach (OperationInformationJson operation in renderInformationJson.operations)
            {
                Quaternion rotation = Quaternion.Euler(operation.rotation.x,operation.rotation.y,operation.rotation.z);
                RotationType rotationType = operation.rotationType switch
                {
                    "random" => RotationType.Random,
                    "initial" => RotationType.Initial,
                    "custom" => RotationType.Custom,
                    "computed" => RotationType.Computed,
                    _ => RotationType.Initial
                };
                renderInformation.Add(new RenderInformation(model,operation.simulationSpeed,operation.simulationTime,operation.amount,rotationType,rotation));
            }
        }
        return renderInformation.ToArray();
    }

    private class RenderInformationJson
    {
        public string inputFolder { get; set; }
        public string outputFolder { get; set; }
        public OperationInformationJson[] operations { get; set; }
    }
    private class OperationInformationJson
    {
        public float simulationSpeed { get; set; }
        public float simulationTime { get; set; }
        public int amount { get; set; }
        
        public string rotationType { get; set; }
        public rotationInformation rotation { get; set; }

        public class rotationInformation
        {
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
        }

        public OperationInformationJson()
        {
            simulationSpeed = 1;
            rotation = new rotationInformation
            {
                x = 0,
                y = 0,
                z = 0
            };
        }
    }
}


