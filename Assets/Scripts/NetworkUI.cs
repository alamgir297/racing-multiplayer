using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour {

    public void StartHost() {
        NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = false;
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient() {
        NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = false;
        NetworkManager.Singleton.StartClient();
    }
    public void StartServer() {
        //NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = false;
        NetworkManager.Singleton.StartServer();
    }

}
