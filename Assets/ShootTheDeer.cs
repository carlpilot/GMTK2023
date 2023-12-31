using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootTheDeer : MonoBehaviour
{
    public GameObject bulletPrefab;
    public AudioSource gunshotSFX;
    public AudioSource reloadSFX;

    List<GameObject> bullets = new List<GameObject>();

    public float bulletSpeed = 10f;
    public float bulletLife = 5f;
    public float soundRange = 100f;

    PlayerMovement pm;

    private float cooldown = 3f;
    private float cooldownTimer = 0.5f;
    private Animator animator;
    private bool aiming = false;

    void Awake()
    {
        pm = GetComponent<PlayerMovement>();
    }
    
    void Start()
    {
        animator = transform.GetChild(0).gameObject.GetComponentsInChildren<Animator>()[0];
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0f)
        {
            animator.Play("aim");
            aiming = true;
        }
        if (Input.GetMouseButtonUp(0) && aiming)
        {
            aiming = false;
            animator.SetTrigger("ShotFired");

            gunshotSFX.Play();
            cooldownTimer = cooldown;
            GameObject bullet = Instantiate(bulletPrefab, pm.camera.position, pm.camera.rotation);
            bullets.Add(bullet);
            StartCoroutine(UpdateBullet(bullet));

            // Scare the deer
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, soundRange);
            foreach (Collider c in hitColliders) {
                if (c.gameObject.tag == "Deer" && c.transform.parent.gameObject.GetComponent<DeerMovement>() != null) {
                    c.transform.parent.gameObject.GetComponent<DeerMovement>().flee(transform.position);
                }
            }

            StartCoroutine(ReloadSounds());
        }
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer < 0f) cooldownTimer = 0f;
    }

    void OnDisable()
    {
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }
        bullets.Clear();
    }

    IEnumerator UpdateBullet(GameObject bullet){
        var timer = 0f;
        bool justCollided = false;
        while (timer < bulletLife)
        {
            bullet.transform.position += bullet.transform.forward * bulletSpeed * Time.deltaTime;
            timer += Time.deltaTime;

            if (justCollided) break;

            // Check for bullet collisions using raycasts
            RaycastHit hit;
            if (Physics.Raycast(bullet.transform.position, bullet.transform.forward, out hit, bulletSpeed * Time.deltaTime))
            {
                if (hasTaggedParent(hit.collider.gameObject, "AIDeer"))
                {
                    // Hit the deer
                    Debug.Log("Hit the deer");
                    GameObject.Find("GameManager").GetComponent<CurrentGameManager>().ShootDeer();
                }
                justCollided = true;
            }

            yield return null;
        }
        if (bullets.Contains(bullet))
        {
            bullets.Remove(bullet);
            Destroy(bullet);
        }
    }

    bool hasTaggedParent(GameObject obj, string tag)
    {
        if (obj.tag == tag)
        {
            return true;
        }
        else if (obj.transform.parent != null)
        {
            return hasTaggedParent(obj.transform.parent.gameObject, tag);
        }
        else
        {
            return false;
        }
    }

    IEnumerator ReloadSounds()
    {
        yield return new WaitForSeconds(2f);
        reloadSFX.Play();
    }
}
