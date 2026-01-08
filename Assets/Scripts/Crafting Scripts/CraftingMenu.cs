using UnityEngine;

public class CraftingMenu : MonoBehaviour
{
    public enum StationType { Default, Workbench, Forge }

    [SerializeField] private GameObject menuRoot;      // whole crafting UI panel
    [SerializeField] private GameObject defaultList;
    [SerializeField] private GameObject workbenchList;
    [SerializeField] private GameObject forgeList;

    public void Open(StationType type)
    {
        menuRoot.SetActive(true);

        defaultList.SetActive(type == StationType.Default);
        workbenchList.SetActive(type == StationType.Workbench);
        forgeList.SetActive(type == StationType.Forge);
    }

    public void Close()
    {
        menuRoot.SetActive(false);
    }
}
