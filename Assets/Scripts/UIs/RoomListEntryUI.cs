using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Controla una fila de la lista de salas disponibles: nombre + "jugadores/max".
// Se deshabilita sola si la sala está llena o cerrada (partida ya en curso).
public class RoomListEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject lockIcon; // se prende si la sala tiene contraseña
    [SerializeField] private TMP_Text stateText;  // opcional: "Llena" / "En curso"

    // Avisa con el nombre de la sala Y si tiene contraseña, para que el popup de unión
    // sepa si tiene que pedir clave o no.
    public void Setup(string roomName, int playerCount, int maxPlayers, bool hasPassword, bool isOpen, Action<string, bool> onSelected)
    {
        if (roomNameText != null) roomNameText.text = roomName;
        if (playerCountText != null) playerCountText.text = $"{playerCount}/{maxPlayers}";
        if (lockIcon != null) lockIcon.SetActive(hasPassword);

        bool isFull = playerCount >= maxPlayers;
        bool isJoinable = isOpen && !isFull;

        if (selectButton != null)
        {
            selectButton.interactable = isJoinable;
        }

        if (stateText != null)
        {
            if (!isOpen) stateText.text = "En curso";
            else if (isFull) stateText.text = "Llena";
            else stateText.text = string.Empty;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelected?.Invoke(roomName, hasPassword));
        }
    }
}