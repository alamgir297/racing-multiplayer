using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : MonoBehaviour {

    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] GameObject _playerPrefab;


    private void OnEnable() {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }
    private void OnDisable() {
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId) {
        if (!NetworkManager.Singleton.IsServer) return;

        int index = NetworkManager.Singleton.ConnectedClientsIds.Count - 1;
        Transform spawn = _spawnPoints[Mathf.Clamp(index, 0, _spawnPoints.Length - 1)];
        //GameObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
        GameObject playerInstance = Instantiate(_playerPrefab, spawn.position, spawn.rotation);
        Debug.Log("spawn: " + spawn.name);

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}
