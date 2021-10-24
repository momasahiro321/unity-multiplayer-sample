using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;

public class Player : NetworkBehaviour
{
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>("");
    private NetworkVariable<int> score = new NetworkVariable<int>(0);
    private float speed = 7.0f;

    [SerializeField]
    private GameObject ballPrefab;
    [SerializeField]
    private Text tagText;

    public void Start()
    {
        if (IsOwner) SetPlayerNameServerRpc(Camera.main.GetComponent<GameManager>().playerName); //サーバーにプレイヤー名を通知
        SetColor();
    }

    public void Update()
    {
        tagText.text = $"{ playerName.Value } | <color=orange>{ score.Value.ToString() }</color>";

        if (!IsOwner) return; //他のクライアントのキャラは動かさないようにする

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector3 direction = mousePosition - transform.position;
        float delta = Mathf.Min(direction.magnitude, speed * Time.deltaTime);
        transform.position += direction.normalized * delta;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            ShotServerRpc(transform.position, direction.normalized); //サーバーに弾の発射を通知
        }
    }

    private void SetColor()
    {
        GetComponent<MeshRenderer>().material.color = IsOwner ? Color.green : Color.white; //自分の操作するキャラは緑色、他人の操作するキャラは白色で表示
    }

    [ServerRpc]
    public void SetPlayerNameServerRpc(string name) //サーバーでプレイヤー名を設定
    {
        playerName.Value = name;
    }

    [ServerRpc]
    public void ShotServerRpc(Vector3 position, Vector3 direction) //サーバーで弾を発射
    {
        if (direction == Vector3.zero) return;
        GameObject ball = Instantiate(ballPrefab, position + 1.2f * direction, Quaternion.identity);
        ball.GetComponent<Ball>().Shot(direction);
        ball.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    public void DecrementScoreServerRpc() //サーバーでスコアを変更
    {
        score.Value--;
    }

    [ClientRpc]
    public void DamageClientRpc() //ダメージを受けたときのクライアントでの処理
    {
        if (IsOwner) DecrementScoreServerRpc();
        GetComponent<MeshRenderer>().material.color = Color.red; //赤くなる

        //0.1秒後に色を戻す
        StartCoroutine(DelayMethod(0.1f, () =>
        {
            SetColor();
        }));
    }

    private IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}
