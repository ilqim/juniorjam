using UnityEngine;
using UnityEngine.Events;

// Dikeyde sabit duran, fanlarla sadece yatayda hareket eden yaprak.
// Düşme hissi arka planın ve engellerin yukarı kaymasıyla verilir (Scroller.cs).
[RequireComponent(typeof(Rigidbody2D))]
public class Leaf : MonoBehaviour
{
    [Header("Yatay hareket")]
    [Tooltip("Yatayda ulaşabileceği en yüksek hız")]
    public float maxHorizontalSpeed = 6f;

    [Tooltip("Fan kapalıyken yatay hızın ne kadar çabuk sönümleneceği")]
    public float horizontalDrag = 2f;

    [Header("Sınırlar")]
    [Tooltip("Yaprağın gidebileceği en sol X")]
    public float minX = -7.5f;

    [Tooltip("Yaprağın gidebileceği en sağ X")]
    public float maxX = 7.5f;

    [Header("Fanlar")]
    [Tooltip("Boş bırakılırsa sahnedeki tüm fanlar otomatik bulunur")]
    public Fan[] fans;

    [Header("Fail")]
    [Tooltip("Bu tag'e sahip objeye çarpınca oyun biter")]
    public string failTag = "Ground";

    [Tooltip("Fail olunca tetiklenir (fail cutscene, skor kaydı vb. buraya bağlanır)")]
    public UnityEvent onFail;

    Rigidbody2D rb;
    float horizontalVelocity;
    bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;        // Yerçekimi yok
        // Dikey konumu ve dönüşü kilitle, yaprak sadece sağa sola gidebilsin
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        if (fans == null || fans.Length == 0)
            fans = FindObjectsByType<Fan>(FindObjectsSortMode.None);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Tüm açık fanların yatay itmesini topla
        float push = 0f;
        foreach (Fan fan in fans)
            push += fan.GetPush(rb.position);

        // İtme varsa hızlan, yoksa yavaşça dur
        horizontalVelocity += push * Time.fixedDeltaTime;
        horizontalVelocity = Mathf.MoveTowards(horizontalVelocity, 0f, horizontalDrag * Time.fixedDeltaTime);
        horizontalVelocity = Mathf.Clamp(horizontalVelocity, -maxHorizontalSpeed, maxHorizontalSpeed);

        // Sınıra ulaştıysa o yöne gitmeyi durdur
        float nextX = rb.position.x + horizontalVelocity * Time.fixedDeltaTime;
        if ((nextX <= minX && horizontalVelocity < 0f) || (nextX >= maxX && horizontalVelocity > 0f))
        {
            horizontalVelocity = 0f;
            rb.position = new Vector2(Mathf.Clamp(rb.position.x, minX, maxX), rb.position.y);
        }

        // Dikeyde hareket yok
        rb.linearVelocity = new Vector2(horizontalVelocity, 0f);
    }

    // Sahne penceresinde sınırları kırmızı çizgiyle gösterir.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(minX, transform.position.y - 20f), new Vector3(minX, transform.position.y + 20f));
        Gizmos.DrawLine(new Vector3(maxX, transform.position.y - 20f), new Vector3(maxX, transform.position.y + 20f));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag(failTag))
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            onFail?.Invoke();
        }
        else
        {
            // Engele yandan çarpınca yatay hızı sıfırla, duvara yapışıp titremesin
            horizontalVelocity = 0f;
        }
    }
}