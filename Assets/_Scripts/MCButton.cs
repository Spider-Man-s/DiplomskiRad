using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class MCButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClicked);
    }

    private void Update()
    {
        if (FusionBoot.Instance == null)
            return;

        var runner = FusionBoot.Instance.Runner;
        if (runner == null || !runner.IsRunning)
            return;

        bool shouldBeInteractable = !runner.IsSharedModeMasterClient;

        if (button.interactable != shouldBeInteractable)
            button.interactable = shouldBeInteractable;
    }

    private void OnClicked()
    {
        if (MasterClientHandover.Instance == null)
        {
            Debug.LogWarning("MCButton: MasterClientHandover is not spawned yet.");
            return;
        }

        MasterClientHandover.Instance.RequestMaster();
    }
}