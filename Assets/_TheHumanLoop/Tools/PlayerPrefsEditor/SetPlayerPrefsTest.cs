using UnityEngine;

public class SetPlayerPrefsTest : MonoBehaviour
{
    [SerializeField] private string _keyToSet;
    [SerializeField] private string _valueToSet;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerPrefs.SetString(_keyToSet, _valueToSet);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
