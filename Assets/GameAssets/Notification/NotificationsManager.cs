using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationsManager : MonoBehaviour
{
    public GameObject notificationPrefab;
    public Transform notificationsContainer;
    public GameObject moreNotificationsIndicator;
    public TMP_Text moreNotificationsText;

    public void RefreshNotifications(List<Prompt> prompts)
    {
        // Clear existing notifications
        foreach (Transform child in notificationsContainer)
        {
            Destroy(child.gameObject);
        }

        // Display up to 3 notifications
        int displayCount = Mathf.Min(3, prompts.Count);
        for (int i = 0; i < displayCount; i++)
        {
            Prompt prompt = prompts[i];
            GameObject notificationGO = Instantiate(notificationPrefab, notificationsContainer);
            NotificationUI notificationUI = notificationGO.GetComponent<NotificationUI>();
            notificationUI.SetNotification(prompt.senderName, prompt.message);
        }

        // Show "more" indicator if there are more than 3 notifications
        if (prompts.Count > 3)
        {
            moreNotificationsIndicator.SetActive(true);
            moreNotificationsText.text = $"+{prompts.Count - 3}";
        }
        else
        {
            moreNotificationsIndicator.SetActive(false);
        }
    }
}
