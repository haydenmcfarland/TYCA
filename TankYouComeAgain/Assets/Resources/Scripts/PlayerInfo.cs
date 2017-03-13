using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prototype.NetworkLobby;
using UnityEngine.Networking;

public class PlayerInfo : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        Player tankPlayer = gamePlayer.GetComponent<Player>();
        tankPlayer.playerName = lobby.playerName;
        tankPlayer.playerColor = lobby.playerColor;
    }
}