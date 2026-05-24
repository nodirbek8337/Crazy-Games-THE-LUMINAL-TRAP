using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMyModelView : MonoBehaviour
{
    public GameObject[] models;
    void Start()
    {
        if (models == null || models.Length == 0) return;

        int savedIndex = PlayerPrefs.GetInt("TheCallOfDarknessPlayerModelIndex", 0);
        SetActiveModel(savedIndex);
    }

    public void SetActiveModel(int index)
    {
        if (index < 0 || index >= models.Length) return;

        for (int i = 0; i < models.Length; i++)
        {
            models[i].SetActive(i == index);
        }
    }
}
