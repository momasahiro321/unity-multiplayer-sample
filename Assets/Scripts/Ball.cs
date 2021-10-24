using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Ball : NetworkBehaviour
{
    private float lifeTime = 3.0f;
    private Vector3 direction;
    private float speed = 15.0f;

    public void Start()
    {
        if (IsServer)
        {
            Destroy(gameObject, lifeTime); //lifeTime秒後に自分自身を削除
        }
    }

    public void Update()
    {
        if (IsServer)
        {
            transform.position += direction * Time.deltaTime;
        }
    }

    public void Shot(Vector3 normalizedDirection)
    {
        this.direction = normalizedDirection * speed;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            Player player = other.GetComponent<Player>();
            if (player == null) return;
            player.DamageClientRpc(); //クライアントにダメージを受けたことを通知
            Destroy(gameObject);
        }
    }
}
