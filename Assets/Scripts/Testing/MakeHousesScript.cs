using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MakeHousesScript : MonoBehaviour
{

    public float cdTime;
    public Transform boxTransform;
    public ChunkNode debugChunk;
    float currentCd = 0f;

    // Start is called before the first frame update
    void Start()
    {
        currentCd = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCd <= 0)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard.hKey.isPressed)
            {
                var targetPosition = boxTransform.position;
                var collision = debugChunk.FindRegionIfAny(targetPosition);
                if (!(collision is null) && collision.RegionLabel == RegionLabel.village)
                {
                    currentCd += cdTime;
                    debugChunk.DebugCreateHut(boxTransform, collision);
                }
            }
            if (keyboard.gKey.isPressed)
            {
                var targetPosition = boxTransform.position;
                var collision = debugChunk.FindRegionIfAny(targetPosition);
                if (collision is null)
                {
                    currentCd += cdTime;
                    debugChunk.DebugCreateVillage(boxTransform.position);
                }
            }
        }
        else
        {
            currentCd -= Time.deltaTime;
        }
        
    }
    
}
