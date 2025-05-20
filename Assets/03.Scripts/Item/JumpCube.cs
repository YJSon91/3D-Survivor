using UnityEngine;

public class JumpCube : MonoBehaviour
{
    public float jumpForce = 800f;

    private void OnCollisionEnter(Collision collision)
    {
        // �÷��̾�� �浹���� ���� ����
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                playerRb.velocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);
                playerRb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange); // �� Ȯ���ϰ� Ʀ
            }
        }
    }
}