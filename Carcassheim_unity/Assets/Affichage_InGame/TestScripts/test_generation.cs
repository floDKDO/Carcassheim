using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_generation : MonoBehaviour
{

    public TuileRepre model;
    public Texture textu;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("t"))
        {
            generate();
        }
    }

    private void generate()
    {
        TuileRepre mod = Instantiate<TuileRepre>(model);
        Renderer red = mod.model.GetComponent<Renderer>();
        Material mat = red.materials[2];
        mod.Id = -1;
        mat.mainTexture = textu;
    }
}
