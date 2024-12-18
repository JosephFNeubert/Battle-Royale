using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;

    private float lastShootTime;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    public void TryShoot()
    {
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
        {
            return;
        }

        curAmmo--;
        lastShootTime = Time.time;

        // Update ammo UI

        player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.position, Camera.main.transform.forward);
    }

    [PunRPC]
    void SpawnBullet (Vector3 pos, Vector3 dir)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
        bulletScript._rb.velocity = dir * bulletSpeed;
    }
}
