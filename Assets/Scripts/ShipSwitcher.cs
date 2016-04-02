using UnityEngine;
using System.Collections;

public class ShipSwitcher : MonoBehaviour {

    public ChunkMap CurrentShip = null;

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10.0f))
        {
            Debug.Log("Raycast OK", hit.collider);
            var ship = hit.collider.gameObject.GetComponentInParent<ChunkMap>();
            if (ship != null)
                CurrentShip = ship;
        }
    }
}
