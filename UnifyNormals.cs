using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Gokhan Ergin Eryildir - www.ergin3d.com

public class UnifyNormals : MonoBehaviour
{

    public GameObject TargetMeshToGetNormals;
    public float DistanceThreshold = 0.001f;
    public bool ShowModifiedNormals = false;

    [Range(0.1f, 10.0f)]
    public float length = 1f;

    public string Note = "Please Enable, Read/Write Enabled from Model Import Settings";

    private Mesh thismesh;
    private Mesh othermesh;

    private Vector3[] thisNormals;
    private Vector3[] thisVertices;
    private Vector3[] thisVerticesWorldCoord;

    private Vector3[] otherNormals;
    private Vector3[] otherVertices;
    private Vector3[] otherVerticesWorldCoord;

    private List<int> thisoverlappingVertices = new List<int>();

    private bool Error = false;
    private bool overlapFound = false;

    private void Start()
    {
        if (this.GetComponent<MeshFilter>()) thismesh = this.GetComponent<MeshFilter>().sharedMesh; // check if this GameObject has a MeshFilter
        else if (this.GetComponent<SkinnedMeshRenderer>()) thismesh = this.GetComponent<SkinnedMeshRenderer>().sharedMesh; // or SkinnedMeshRenderer
        else
        {
            Debug.Log("Error 1: This Script is not attached to suitable Mesh");
            Error = true;
        }

        if (TargetMeshToGetNormals != null)
        {
            if (TargetMeshToGetNormals.GetComponent<MeshFilter>()) othermesh = TargetMeshToGetNormals.GetComponent<MeshFilter>().sharedMesh;
            else if (this.GetComponent<SkinnedMeshRenderer>()) othermesh = TargetMeshToGetNormals.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            else
            {
                Debug.Log("Error 2: Target Mesh is not a suitable Mesh");
                Error = true;
            }

        }
        else
        {
            Debug.Log("Error 3: Target Mesh is not specified");
            Error = true;
        }

        if (!Error)
        {

            thisNormals = thismesh.normals;
            thisVertices = thismesh.vertices;
            thisVerticesWorldCoord = thismesh.vertices;

            for (int i = 0; i < thisVertices.Length; i++) // Converting this mesh's vertices to world coordinates.
            {                                             // Orginal vertex locations are relative to GameObject
                thisVerticesWorldCoord[i] = transform.TransformPoint(thisVertices[i]); // And unless converted (moved to GameObject's location)
            }                                             // 2 meshes won't align with eachother.

            otherNormals = othermesh.normals;
            otherVertices = othermesh.vertices;
            otherVerticesWorldCoord = othermesh.vertices;

            for (int i = 0; i < otherVertices.Length; i++) // Converting target mesh's vertices to world coordinates.
            {
                otherVerticesWorldCoord[i] = TargetMeshToGetNormals.GetComponent<Transform>().TransformPoint(otherVertices[i]);
            }



            for (int i = 0; i < thisVertices.Length; i++) 
            { 
                for (int j = 0; j < otherVertices.Length; j++) 
                {
                    // Measure distance between this mesh's vertices and other mesh's vertices one by one.
                    float distanceBetweenVertices = Vector3.Distance(thisVerticesWorldCoord[i], otherVerticesWorldCoord[j]);
                    
                    if (distanceBetweenVertices <= DistanceThreshold) // and if the distance is smaller than the treshold
                    {                                                 
                        thisoverlappingVertices.Add(i); // store indices to show normals for debug purposes.
                        thisNormals[i] = otherNormals[j]; // we set the normal of this vertex to target mesh's vertex normal.
                        overlapFound = true;
                    }
                }
            }

            if (overlapFound) thismesh.normals = thisNormals; // than we apply modified normals to mesh
            else Debug.Log("No close vertices found, please check threshold"); 

        }
        else Debug.LogWarning("There is an Error, Please check settings");


    }

    private void OnDrawGizmos() // Show Gizmos must be enabled from the Editor Window
    {
        if (EditorApplication.isPlaying) // Only show normals during Gameplay Mode
        { 
            if (ShowModifiedNormals)
            {
                if (overlapFound)
                {
                    Gizmos.color = Color.red;
                    for (int i = 0; i < thisoverlappingVertices.Count; i++)
                    {
                        int currentIndex = thisoverlappingVertices[i];

                        Vector3 NormalVector = new Vector3();
                        NormalVector = thisVerticesWorldCoord[currentIndex] + thisNormals[currentIndex] / (101f - (length * 10));
                        Gizmos.DrawLine(thisVerticesWorldCoord[currentIndex], NormalVector);
                    }
                }
            }
        }

    }
}
