using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

// Varför får jag errors om klaserna ligger utanför MonoBehavior/inte har det?

public class LinkedList : MonoBehaviour
{

    // TESTS BELOW
    private void Awake() // Test LinkedList on awake
    {

        LinkedListTest TestLL = new LinkedListTest();

        TestLL.Add(5);
        Debug.Log("Size is: " +TestLL.ReturnSize());
        Debug.Log("Data of newest element is : " +TestLL.latestElement.data);

        TestLL.Add(0);
        Debug.Log("Size is: " +TestLL.ReturnSize());
        Debug.Log("Data of newest element is : " +TestLL.latestElement.data);

        TestLL.Add(6);
        TestLL.Add(10);
        TestLL.Add(250);
        TestLL.Add(-6);

        Debug.Log("Visual representation (string) of list is: "+TestLL.ReturnStringRepresentation());

        Debug.Log("At index 0 the data is: "+TestLL.Get(0));
        Debug.Log("At index 3 the data is: "+TestLL.Get(3));
        Debug.Log("At index 5 the data is: "+TestLL.Get(5));

    }
    // TESTS CONCLUDED

    // Linked list implementation based on my presented solution to task 1 course Grundläggande Datalogi 2024 (Viktor Gajdamowicz Bolin)
    public class ListElement
    {

        public int data; // Init the data which can be of type int
        public ListElement nextElement; // Init the next element pointer of type ListElement

        public ListElement(int dataToStore) // Constructor of the list element
        {
            data = dataToStore;
        }

    }
    public class LinkedListTest // Slight difference in implementation, only tracking bottom (latest) element. Element in front will be signified by having nextElement as null. To return loop through all and return (can have check if value is null)
    {
        public ListElement latestElement;
        public int size;

        //public LinkedListTest(){} // Empty initalizer, no input required

        public void Add(int newData)
        {

            ListElement newElement = new ListElement(newData); //Creates new ListElement from given function data input

            if (latestElement == null) //Default value of latestElement (initialized but unassigned) is null
            {
                latestElement = newElement;
                size += 1;
            }

            else // When list is not empty, reassign newElement.nextElement to latestElement, then latestElement to newElement
            {
                newElement.nextElement = latestElement;
                latestElement = newElement;
                size += 1;
            }
        }

        public int ReturnSize()
        {
            return size;
        }

        public string ReturnStringRepresentation()
        {
            string stringRepresentationOfList = "";

            stringRepresentationOfList += "]";

            int loopCounter = 0;

            ListElement currentElementInTraversal = latestElement;


            while (loopCounter < size) //Constructing visual representation from back to front to align list order correctly in string
            {
                stringRepresentationOfList = $"{currentElementInTraversal.data}{stringRepresentationOfList}";
                currentElementInTraversal = currentElementInTraversal.nextElement;

                loopCounter += 1;

                if (loopCounter != size) //Add commas before all elements except for first
                {
                    stringRepresentationOfList = $", {stringRepresentationOfList}";
                }

            }

            stringRepresentationOfList = $"[{stringRepresentationOfList}";

            return stringRepresentationOfList;
        }

        public int Get(int indexToFind)
        {
            int depthCounter = 0;

            int ourDepth = size - indexToFind; // Depth is "from back", meaning a list of len 10, max index 9 and indexToFind of 0 would give a depth of 10. indexToFind of 9 gives depth of 1 (first loop)

            ListElement currentElementInTraversal = latestElement;

            int dataToReturn = currentElementInTraversal.data; //Be aware: if loop does not initalize data of latestElement will be returned

            while (depthCounter < ourDepth && indexToFind >= 0 && indexToFind+1 <= size) //Non-total error management (to be fixed). Loop will only engage if correct indexToFind, no errors are thrown otherwise
            {
                dataToReturn = currentElementInTraversal.data; //Data is taken from latest
                currentElementInTraversal = currentElementInTraversal.nextElement; // Will point to null at final next element (at first list element)

                depthCounter += 1;
            }

            return dataToReturn;
        }


    }
}