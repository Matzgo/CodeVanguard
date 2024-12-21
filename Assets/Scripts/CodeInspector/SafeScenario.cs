using System.Collections.Generic;
using UnityEngine;

public class SafeScenario : MonoBehaviour
{
    [SerializeField]
    GameObject _safeDoor;

    [SerializeField]
    AudioSource _audioSource;

    [SerializeField]
    AudioClip _denySFX;

    [SerializeField]
    AudioClip _validSFX;
    [SerializeField]
    List<GameObject> _insideItems;

    bool _open = false;

    private void Awake()
    {
        for (int i = 0; i < _insideItems.Count; i++)
        {
            _insideItems[i].SetActive(false);
        }
    }

    public void ResetSafe()
    {
        _safeDoor.SetActive(true);
    }

    public void OpenSafe()
    {
        _open = true;
        _safeDoor.SetActive(false);
        for (int i = 0; i < _insideItems.Count; i++)
        {
            _insideItems[i].SetActive(true);
        }
        _audioSource.PlayOneShot(_validSFX);

    }

    public void SafeAlarm()
    {
        _audioSource.PlayOneShot(_denySFX);
    }

    public bool CheckValid()
    {
        if (_open)
            return true;

        return false;
    }

}
