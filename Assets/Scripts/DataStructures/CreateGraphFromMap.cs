// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;

// public class CreateGraphFromMap : MonoBehaviour
// {
//     // Perhaps rename main class, here we could store the graph representing the map as a variable, then change it/update it using the functions
//     Graph graphOfMap = new Graph();

//     Array syncedOldArrayOfMap; //Variable to store matching old array (or whatever data type) of map for update comparison

//     public Graph CreateGraphFromMapMethod(Array arrayRepresentingMap) // Don't know if map is an array or how it is stored
//     {

//         // Chech that graphOfMap is empty, else throw error

//         // Add nodes to this graph according to the array (or from whatever type the map is).

//         return graphOfMap;

//     }

//     public Graph UpdateMapGraphAfterChunkLoad(Array newMapOrJustNewChunkInfo) // Either this function can take the old graph from the main class container, or it can get it from a separate script (that calls the function)
//     {

//         // Compare old and new array, if truly updated proceed, otherwise throw error

//         // Locate and extract data from all changes, or if we can designate the new areas separately we can save time. Otherwise we might as well re-construct the entire graph with each update.
//         // So: requirement from map is that is saves and or knows whatever new chunks have been added. Perhaps we dont need to feed the entire map after chunk updates, only on creation
//         // and we can feed only the new chunks after each generation?
//         // If we chose only update we will need to update the old map array too, or feed it in for storage in this script's container. Shouldn't be computationally intensive anyways.

//         // Update old graph by adding nodes and edges wherever needed according to the changes in the new version or alternatively the new chunk

//         return graphOfMap;

//     }


//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }
