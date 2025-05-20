using UnityEngine;

public class JumpCube : MonoBehaviour
{
    public float jumpForce = 800f;

    private void OnCollisionEnter(Collision collision)
    {
        // 플레이어와 충돌했을 때만 동작
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            Animator playerAnimator = collision.gameObject.GetComponent<Animator>();

            if (playerRb != null)
            {
                playerAnimator.SetTrigger("Jump"); // 점프 애니메이션 트리거
                playerRb.velocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);
                playerRb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange); // 더 확실하게 튐
            }
        }
    }
}