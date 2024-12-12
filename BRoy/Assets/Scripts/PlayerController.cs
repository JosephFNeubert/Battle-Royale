using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [Header("Info")]
    public int id;
    private int curAttackerId;

    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public int currHp;
    public int maxHp;
    public int kills;
    public bool dead;

    private bool flashingDamage;

    [Header("Components")]
    public Rigidbody _rb;
    public Player photonPlayer;
    public MeshRenderer _mr;
    public PlayerWeapon weapon;

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            _rb.isKinematic = true;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine || dead)
        {
            return;
        }

        Move();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            weapon.TryShoot();
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Calculate a direction relative to where the player is facing
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = _rb.velocity.y;

        _rb.velocity = dir;
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 1.5f))
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    [PunRPC]
    public void TakeDamage (int attackerId, int damage)
    {
        if (dead)
        {
            return;
        }

        currHp -= damage;
        curAttackerId = attackerId;

        photonView.RPC("DamageFlash", RpcTarget.Others);
        // Update health bar UI
        
        if (currHp <= 0)
        {
            photonView.RPC("Die", RpcTarget.All);
        }
    }

    [PunRPC]
    void die()
    {

    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
        {
            return;
        }

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;
            Color defaultColor = _mr.material.color;
            _mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            _mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }
}
