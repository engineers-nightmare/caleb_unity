using UnityEngine;
using System.Collections;

public class Inventory : MonoBehaviour {
    public GameObject[] Slots = new GameObject[Constants.PlayerInventorySize];
    public int ActiveSlot = 0;

    void Update()
    {
        // Note: /really/ don't want any smoothing here.
        var scroll = -Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (Slots[ActiveSlot] != null)
                Slots[ActiveSlot].SetActive(false);

            ActiveSlot = (ActiveSlot + Slots.Length + (scroll > 0 ? 1 : -1)) % Slots.Length;

            if (Slots[ActiveSlot] != null)
                Slots[ActiveSlot].SetActive(true);
        }
    }

    void OnGUI()
    {
        var y = 50;
        for (var i = 0; i < Slots.Length; i++)
        {
            var go = Slots[i];
            if (ActiveSlot == i)
                GUI.Label(new Rect(10, y, 20, 20), ">");

            GUI.Label(new Rect(20, y, 200, 20), go != null ? go.name : "--");
            y += 20;
        }
    }
}
