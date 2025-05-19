using UnityEngine;

public class EquipTool : Equip
{
    public float attackRate;
    private bool attacking;
    public float attackDistance;
    public float usdStamina;

    [Header("Resource Gathering")]
    public bool doesGatherResources;

    [Header("Combat")]
    public bool doesDealDamage;
    public int damage;

    private Animator animator;
    private Camera camera;

    void Start()
    {
        animator = GetComponent<Animator>();
        camera = Camera.main;
    }

    public override void OnAttackInput()
    {
        if(!attacking)
        {
            if (CharacterManager.Instance.Player.condition.UseStamina(usdStamina))
            {
                attacking = true;
                animator.SetTrigger("Attack");
                Invoke("onCanAttack", attackRate);
            }
           
            
        }
    }

    void onCanAttack()
    {
        attacking = false;
        
    }

    public void OnHIt()
    {
        Ray ray =camera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2));
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, attackDistance))
        {
            if (doesGatherResources&& hit.collider.TryGetComponent(out Resource resource))
            {
                
                    resource.Gather(hit.point, hit.normal);
                
            }
            //if (doesDealDamage)
            //{
            //    Enemy enemy = hit.collider.GetComponent<Enemy>();
            //    if (enemy != null)
            //    {
            //        enemy.TakeDamage(damage);
            //    }
            //}
        }
    }
}