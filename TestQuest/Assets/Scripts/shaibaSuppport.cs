using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shaibaSuppport : MonoBehaviour
{
    Manager _manager;

    public GameObject numberObj;
    public Transform numberStorage;
    List<GameObject> poolingObjs = new List<GameObject>();

    void Start()
    {
        _manager = FindObjectOfType<Manager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(_manager == null)
            {
                return;
            }
            _manager.ForceStop();
        }
    }
    public void AnimPlay(int index)
    {
        StartCoroutine(MoveNumberObj(index));
    }
    IEnumerator MoveNumberObj(int index)
    {
        GameObject go = GetPooledObject(numberObj);
        go.GetComponent<TextMesh>().text = index.ToString();
        go.GetComponent<TextMesh>().color = Color.yellow;
        go.SetActive(true);
        go.GetComponent<Animation>().Play();

        Color old_color = go.GetComponent<TextMesh>().color;
        Color new_color = go.GetComponent<TextMesh>().color;

        while (go.GetComponent<Animation>().isPlaying)
        {
            new_color.a -= 1 * Time.deltaTime;

            go.GetComponent<TextMesh>().color = new_color;
            yield return null;
        }

        go.SetActive(false);
        go.GetComponent<TextMesh>().color = old_color;
    }
    public GameObject GetPooledObject(GameObject go)
    {
        for (int i = 0; i < poolingObjs.Count; i++)
        {
            if (!poolingObjs[i].activeInHierarchy)
            {
                return poolingObjs[i];
            }

        }
        GameObject obj = Instantiate(go);
        obj.SetActive(true);
        obj.transform.SetParent(numberStorage);
        poolingObjs.Add(obj);
        return obj;
    }
}
