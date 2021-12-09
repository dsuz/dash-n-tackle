using UnityEngine;
using Photon.Pun;

/// <summary>
/// ゲームを管理する。
/// 接続 > 部屋に入って以降の管理をする。
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] Transform[] _spawnPoints = default;
    [SerializeField] string _playerPrefabName = "";

    public void StartGame()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log($"My ActorNumber: {actorNumber}");
        Transform spawnPoint = _spawnPoints[(actorNumber - 1) % _spawnPoints.Length];
        Debug.Log($"Spawns at {spawnPoint.name}");
        GameObject player = PhotonNetwork.Instantiate(_playerPrefabName, spawnPoint.position, spawnPoint.rotation);
        player.name = $"Player{actorNumber}";
        player.GetComponent<PlayerController>().enabled = true;
    }
}
