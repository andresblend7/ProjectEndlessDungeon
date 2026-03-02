using UnityEngine;

public class AlwaysMoveFowards : MonoBehaviour
{
    public float speed = 5f;

    void Start()
    {
        
    }

    void Update()
    {
         transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
