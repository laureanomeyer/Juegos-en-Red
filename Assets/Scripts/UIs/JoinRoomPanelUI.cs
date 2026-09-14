using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomPanelUI : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private RoomListEntryUI roomListEntryPrefab;

    public event Action OnBackRequested;
    public event Action<string, bool> OnRoomSelected;

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();

    private void Awake()
    {
        backButton.onClick.AddListener(() => OnBackRequested?.Invoke());
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void Refresh(IReadOnlyDictionary<string, RoomInfo> rooms)
    {
        foreach (var entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        if (rooms == null || roomListEntryPrefab == null || roomListContent == null) return;
        if (PhotonManager.Instance == null) return;

        foreach (var kvp in rooms)
        {
            RoomInfo info = kvp.Value;
            if (info.RemovedFromList) continue;

            RoomListEntryUI entryUI = Instantiate(roomListEntryPrefab, roomListContent);
            bool hasPassword = PhotonManager.Instance.RoomHasPassword(info);

            entryUI.Setup(info.Name, info.PlayerCount, info.MaxPlayers, hasPassword, info.IsOpen,
                (roomName, pwd) => OnRoomSelected?.Invoke(roomName, pwd));

            spawnedEntries.Add(entryUI.gameObject);
        }
    }
}