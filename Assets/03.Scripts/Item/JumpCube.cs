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

            if (playerRb != null)
            {
                playerRb.velocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);
                playerRb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange); // 더 확실하게 튐
            }
        }
    }
}