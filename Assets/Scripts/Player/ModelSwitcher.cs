using UnityEngine;

public class ModelSwitcher : MonoBehaviour
{
    [Header("All Models (child objects)")]
    public GameObject[] models;

    public int currentIndex = 0;
    public Animator CurrentAnimator { get; private set; }

    void Awake()
    {
        RefreshAnimatorReference();
    }

    void Start()
    {
        if (models == null || models.Length == 0) return;

        SetActiveModel(currentIndex);
    }

    public void SetActiveModel(int index)
    {
        if (models == null || models.Length == 0)
        {
            CurrentAnimator = null;
            return;
        }

        if (index < 0 || index >= models.Length)
        {
            Debug.LogWarning($"{name}: Model index {index} is out of range. Models count: {models.Length}.");
            return;
        }

        for (int i = 0; i < models.Length; i++)
        {
            GameObject model = models[i];
            if (model == null) continue;

            bool isActive = (i == index);
            model.SetActive(isActive);
        }

        currentIndex = index;
        RefreshAnimatorReference();
    }

    void LateUpdate()
    {
        if (models == null || models.Length == 0 || currentIndex < 0 || currentIndex >= models.Length) return;

        GameObject activeModel = models[currentIndex];
        if (activeModel != null && activeModel.activeInHierarchy)
        {
            activeModel.transform.position = transform.position;
            activeModel.transform.rotation = transform.rotation;
        }
    }

    private void RefreshAnimatorReference()
    {
        if (models == null || models.Length == 0 || currentIndex < 0 || currentIndex >= models.Length)
        {
            CurrentAnimator = null;
            return;
        }

        GameObject activeModel = models[currentIndex];
        if (activeModel == null)
        {
            CurrentAnimator = null;
            return;
        }

        CurrentAnimator = activeModel.GetComponent<Animator>();
        if (CurrentAnimator == null)
            CurrentAnimator = activeModel.GetComponentInChildren<Animator>(true);
    }
}
