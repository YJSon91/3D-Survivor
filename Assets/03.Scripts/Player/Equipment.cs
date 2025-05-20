using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Equipment : MonoBehaviour
{

    public Equip curEquip;
    public Transform equipParent;

    private PlayerController controller;
    private PlayerCondition condition;
    private Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        condition = GetComponent<PlayerCondition>();
        controller = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
    }

    public void EquipNew(ItemData data)
    {
        Unequip();
        curEquip = Instantiate(data.equipPrefab, equipParent).GetComponent<Equip>();
      
    }
    public void Unequip()
    {
        if (curEquip != null)
        {
            Destroy(curEquip.gameObject);
            curEquip = null;
        }
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed&&curEquip !=null&&controller.canLook)
        {
            if (curEquip != null)
            {
                curEquip.OnAttackInput();
                animator.SetTrigger("Attack");
            }
        }
    }

   
}
