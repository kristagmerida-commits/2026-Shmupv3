using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is an enum of the various possible weapon types.
/// It also includes a "shield" type to allow a shield PowerUp.
/// Items marked [NI] below are Not Implemented in this book.
/// </summary>
public enum eWeaponType
{
    none,       // The default / no weapon
    blaster,    // A simple blaster
    spread,     // Multiple shots simultaneously
    phaser,     // Heavy blaster - slow, high damage
    missile,    // [NI] Homing missiles
    laser,      // [NI] Damage over time
    shield      // Raise shieldLevel
}


/// <summary>
/// The WeaponDefinition class allows you to set the properties
///   of a specific weapon in the Inspector. The Main class has
///   an array of WeaponDefinitions that makes this possible.
/// </summary>
[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.none;
    [Tooltip("Letter to show on the PowerUp Cube")]
    public string letter;
    [Tooltip("Color of PowerUp Cube")]
    public Color powerUpColor = Color.white;
    [Tooltip("Prefab of Weapon model that is attached to the Player Ship")]
    public GameObject weaponModelPrefab;
    [Tooltip("Prefab of projectile that is fired")]
    public GameObject projectilePrefab;
    [Tooltip("Color of the Projectile that is fired")]
    public Color projectileColor = Color.white;
    [Tooltip("Damage caused when a single Projectile hits an Enemy")]
    public float damageOnHit = 0;
    [Tooltip("Damage caused per second by the Laser [Not Implemented]")]
    public float damagePerSec = 0;
    [Tooltip("Seconds to delay between shots")]
    public float delayBetweenShots = 0;
    [Tooltip("Velocity of individual Projectiles")]
    public float velocity = 50;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    [Tooltip("Setting this manually while playing does not work properly.")]
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public float nextShotTime; // Time the Weapon will fire next

    private GameObject weaponModel;
    private Transform shotPointTrans;

    void Start()
    {
        // Set up PROJECTILE_ANCHOR if it has not already been done
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);

        // Call SetType() for the default _type set in the Inspector
        SetType(_type);

        // Find the fireEvent of a Hero Component in the parent hierarchy
        Hero hero = GetComponentInParent<Hero>();
        if (hero != null) hero.fireEvent += Fire;
    }

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;
        if (type == eWeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        // Get the WeaponDefinition for this type from Main
        def = Main.GET_WEAPON_DEFINITION(_type);

        // Destroy any old model and then attach a model for this weapon
        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = null;

        // === CHANGE 1:
        if (def.weaponModelPrefab != null)
        {
            weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
            weaponModel.transform.localPosition = Vector3.zero;
            weaponModel.transform.localScale = Vector3.one;
        }

        nextShotTime = 0; // You can fire immediately after _type is set.
    }

    private void Fire()
    {
        // If this.gameObject is inactive, return
        if (!gameObject.activeInHierarchy) return;
        // If it hasn't been enough time between shots, return
        if (Time.time < nextShotTime) return;
        // If there's no definition (shouldn't happen but be safe), return
        if (def == null) return;

        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        switch (type)
        {
            case eWeaponType.blaster:
                p = MakeProjectile();
                p.vel = vel;
                break;

            // === CHANGE 2: spread now fires 5 projectiles at +/-10 and +/-20 ===
            case eWeaponType.spread:
                // Center
                p = MakeProjectile();
                p.vel = vel;

                // +10 degrees
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.vel = p.transform.rotation * vel;

                // -10 degrees
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.vel = p.transform.rotation * vel;

                // +20 degrees
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(20, Vector3.back);
                p.vel = p.transform.rotation * vel;

                // -20 degrees
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-20, Vector3.back);
                p.vel = p.transform.rotation * vel;
                break;

            // CHANGE 3: new phaser (heavy blaster) 
            // Same straight-shot pattern as blaster; its damage and firing
            // rate come from its own WeaponDefinition entry on _MainCamera.
            case eWeaponType.phaser:
                p = MakeProjectile();
                p.vel = vel;
                break;

            // === CHANGE 4: laser case (stub — fires a single slow projectile) ===
            // The projectile prefab for this weapon should be a long thin
            // stretched cube with a trigger collider. See notes below for
            // how to make the continuous-damage part actually work.
            case eWeaponType.laser:
                p = MakeProjectile();
                p.vel = vel;
                break;

            // default: do nothing for none / shield / missile
        }
    }

    private ProjectileHero MakeProjectile()
    {
        // CHANGE 5:
        if (def.projectilePrefab == null)
        {
            Debug.LogWarning("Weapon " + type + " has no projectilePrefab assigned.");
            return null;
        }

        GameObject go;
        go = Instantiate<GameObject>(def.projectilePrefab, PROJECTILE_ANCHOR);
        ProjectileHero p = go.GetComponent<ProjectileHero>();
        if (p == null)
        {
            Debug.LogWarning("projectilePrefab for " + type + " has no ProjectileHero component.");
            Destroy(go);
            return null;
        }

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;
        p.transform.position = pos;

        p.type = type;
        nextShotTime = Time.time + def.delayBetweenShots;
        return (p);
    }
}