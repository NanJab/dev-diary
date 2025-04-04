using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEquip : NetworkBehaviour
{
    public Transform weaponHolder;
    [SerializeField]
    private Weapon currentWeapon;
    [SerializeField]
    private GameObject defaultWeapon;

    [SyncVar(hook = nameof(OnWeaponChanged))]
    private GameObject networkedWeapon;

    public override void OnStartAuthority()
    {
        if(defaultWeapon != null && isLocalPlayer)
        {
            CmdEquipWeapon(defaultWeapon.name);
        }
    }

    private void Update()
    {
        if (!isOwned) return;

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            CmdEquipWeapon(WeaponDatabase.Instance.GetWeaponPrefab("Bat").name);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            CmdEquipWeapon(WeaponDatabase.Instance.GetWeaponPrefab("Shovel").name);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            CmdEquipWeapon(WeaponDatabase.Instance.GetWeaponPrefab("Sword").name);
        }
    }

    [Command] // 서버에서 실행
    public void CmdEquipWeapon(string weaponName)
    {
        //if (!isLocalPlayer) return;

        if (currentWeapon != null)
        {
            NetworkServer.Destroy(currentWeapon.gameObject);
        }

        // 데이터베이스에서 무기 프리팹 가져오기
        GameObject weaponPrefab = WeaponDatabase.Instance.GetWeaponPrefab(weaponName);
        if (weaponPrefab == null) return;

        // 무기 생성 시 부모를 null로 설정하여 월드 좌표계에서 생성
        GameObject newWeapon = Instantiate(weaponPrefab);

        // 네트워크 생성 및 권한 할당
        NetworkServer.Spawn(newWeapon, connectionToClient);
        networkedWeapon = newWeapon;

        ConfigureWeapon(newWeapon);
        currentWeapon = newWeapon.GetComponent<Weapon>();

        WeaponSet(newWeapon);
        RpcEquipWeapon(newWeapon);
    }

    [ClientRpc] // 클라이언트에서 실행
    private void RpcEquipWeapon(GameObject weapon)
    {
        if (weapon == null) return;

        if (isServer && isOwned) return;

        ConfigureWeapon(weapon);
        currentWeapon = weapon.GetComponent<Weapon>();

        WeaponSet(weapon);
        //SetupWeapon(weapon);
    }

    private void ConfigureWeapon(GameObject weapon)
    {
        weapon.transform.SetParent(weaponHolder);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        weapon.transform.localScale = Vector3.one;
    }

    private void OnWeaponChanged(GameObject oldWeapon, GameObject newWeapon)
    {
        if (newWeapon == null) return;

        // 이미 로컬에서 처리된 경우 제외
        if (isOwned) return;

        ConfigureWeapon(newWeapon);
        currentWeapon = newWeapon.GetComponent<Weapon>();

        WeaponSet(newWeapon);
    }

    void WeaponSet(GameObject weapon)
    {
        var stat = GetComponent<PlayerStat>();
        var attackMotion = weapon.GetComponent<WeaponAttackMotion>();
        if (stat != null && attackMotion != null)
        {
            stat.SetWeapon(attackMotion);
        }
    }
}
