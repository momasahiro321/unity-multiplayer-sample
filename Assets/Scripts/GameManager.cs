using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

public class GameManager : MonoBehaviour
{
    private string address = "127.0.0.1";
    private int port = 7777;
    private bool started = false;

    public string playerName = "No Name";

    public void Start()
    {
        playerName = PlayerPrefs.GetString("player_name", playerName);
        address = PlayerPrefs.GetString("address", address);
        port = PlayerPrefs.GetInt("port", port);

        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong id) =>
        {
            if (!NetworkManager.Singleton.IsHost) started = false; //ホストから切断された時
        };
    }

    public void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        GUILayout.Label("Your Player Name");
        playerName = GUILayout.TextField(playerName);
        GUILayout.Label("Server Address");
        address = GUILayout.TextField(address);
        GUILayout.Label("Port");
        port = int.Parse(GUILayout.TextField(port.ToString()));

        if (!started && GUILayout.Button("Start Client"))
        {
            ConnectSettings(address, port);
            NetworkManager.Singleton.StartClient();
        }

        if (!started && GUILayout.Button("Start Host"))
        {
            ConnectSettings(address, port);
            NetworkManager.Singleton.StartHost();
        }

        if (started && GUILayout.Button("Stop"))
        {
            NetworkManager.Singleton.Shutdown();
            started = false;
        }

        GUILayout.EndArea();
    }

    private void ConnectSettings(string address, int port) //IPアドレスとポート番号の設定
    {
        UNetTransport unet = GameObject.Find("NetworkManager").GetComponent<UNetTransport>();
        unet.ConnectAddress = address;
        unet.ConnectPort = port;
        started = true;

        PlayerPrefs.SetString("player_name", playerName);
        PlayerPrefs.SetString("address", address);
        PlayerPrefs.SetInt("port", port);
    }
}
