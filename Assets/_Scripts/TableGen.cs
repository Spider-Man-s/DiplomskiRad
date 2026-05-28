using UnityEngine;

public class TableGen : MonoBehaviour
{
    [Header("Table Dimensions (meters)")]

    [SerializeField]
    private float tableLength = 1.2f;

    [SerializeField]
    private float tableWidth = 0.6f;

    [SerializeField]
    private float tableHeight = 0.05f;

    [SerializeField]
    private Material tableMaterial;

    private void Start()
    {
        GenerateTable();
    }

    private void GenerateTable()
    {
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);

        table.name = "GeneratedTable";

        table.transform.SetParent(transform);

        table.transform.localPosition = Vector3.zero;

        table.transform.localScale = new Vector3(
            tableLength,
            tableHeight,
            tableWidth
        );

        if (tableMaterial != null)
        {
            table.GetComponent<Renderer>().material = tableMaterial;
        }
    }
}