using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditorIdentifierSimulation
{
    static Dictionary<string, int> identifierDict = new Dictionary<string, int>();

    static int counter = 1;
    public static int CheckEditorImage(string guid)
    {
        if (identifierDict.ContainsKey(guid))
            return identifierDict[guid];
        else
        {
            identifierDict.Add(guid, counter);
            ++counter;
            return identifierDict[guid];
        }
    }
}
