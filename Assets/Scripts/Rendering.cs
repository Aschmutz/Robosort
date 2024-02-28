using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Rendering : MonoBehaviour
{
    [SerializeField] private Camera cameraLeft;
    [SerializeField] private Camera cameraRight;
    [SerializeField] private TextureFormat textureFormat;
    [SerializeField] private RenderTexture renderTexture;
    private int m_ImageRenderId;

    // Start is called before the first frame update
    void Start()
    {
        cameraLeft.enabled = false;
        cameraRight.enabled = false;
        m_ImageRenderId = 0;
    }

    /**
     * Renders a single image and saves it to the folder that is specified.
     * The simulation Time is the time that is taken for simulating the physics of the object.
     */
    public IEnumerator RenderSingleImage(string path, float simulationTime, string prefix)
    {
        //simulation delay
        if (simulationTime > 0)
            yield return new WaitForSeconds(simulationTime);
        cameraLeft.enabled = true;
        cameraRight.enabled = true;
        RenderTexture.active = renderTexture;
        cameraLeft.Render();
        cameraRight.Render();

        Texture2D t2d = new Texture2D(renderTexture.width, renderTexture.height,textureFormat,true);
        t2d.ReadPixels(new Rect(0,0,renderTexture.width, renderTexture.height),0,0);
        t2d.Apply();
        byte[] pngPixels = t2d.EncodeToPNG();
        
        SaveRenderToFile(pngPixels,path, prefix,m_ImageRenderId++);
        cameraLeft.enabled = false;
        cameraRight.enabled = false;
    }

    private async void SaveRenderToFile (byte[] imageData, string path, string prefix,int id)
    {
        string fullPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar +path;
        Directory.CreateDirectory(fullPath);
        FileStream f;
        string filePath = fullPath + Path.DirectorySeparatorChar + prefix + id + ".png";
        f = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await f.WriteAsync(imageData,0,imageData.Length);
        f.Close();
        Debug.Log("Succsessfuly wrote Pixel Data to PNG File");
        Console.WriteLine("Saved: "+filePath+".png");
    }
}
