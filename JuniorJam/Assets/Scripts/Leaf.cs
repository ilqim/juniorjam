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

    [Tooltip("Yatay hızın ne kadar çabuk sönümleneceği (yüksek = çabuk durur)")]
    public float damping = 3f;

    [Header("Sınırlar")]
    [Tooltip("Yaprağın gidebileceği en sol X")]
    public float minX = -7.5f;

    [Tooltip("Yaprağın gidebileceği en sağ X")]
    public float maxX = 7.5f;

    [Tooltip("Booster tarafından ayarlanır. 1 = normal hız")]
    [HideInInspector] public float speedMultiplier = 1f;

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
        horizontalVelocity += push * speedMultiplier * Time.fixedDeltaTime;
        // Hıza orantılı sönümleme: zayıf itme bile yaprağı biraz hareket ettirir
        horizontalVelocity *= 1f / (1f + damping * Time.fixedDeltaTime);
        float maxSpeed = maxHorizontalSpeed * speedMultiplier;
        horizontalVelocity = Mathf.Clamp(horizontalVelocity, -maxSpeed, maxSpeed);

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

    // Dal collider'ı "Is Trigger" ise buraya düşer
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDead && other.CompareTag(failTag))
            Fail();
    }

    void Fail()
    {
        isDead = true;
        horizontalVelocity = 0f;
        rb.linearVelocity = Vector2.zero;
        onFail?.Invoke();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag(failTag))
        {
            Fail();
        }
        else
        {
            // Engele yandan çarpınca yatay hızı sıfırla, duvara yapışıp titremesin
            horizontalVelocity = 0f;
        }
    }
}