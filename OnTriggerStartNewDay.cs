using UnityEngine;
using UnityEngine.EventSystems;

public class OnTriggerStartNewDay : MonoBehaviour
{
    public GameObject confirmPanel;
    public PlayerController playerController;
    public GameObject yesButton;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player")) {
            confirmPanel.SetActive(true);
            playerController.CanMove = false;

            // Automatically set the EventSystem's selected object to the Yes button when the panel is shown
            EventSystem.current.SetSelectedGameObject(yesButton.gameObject);
        }
    }
}
