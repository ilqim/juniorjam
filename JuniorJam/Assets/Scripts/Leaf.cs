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

    [Header("Dalgalı hareket (U şekli)")]
    [Tooltip("Yaprak en hızlıyken başlangıç yüksekliğinin ne kadar altına iner. 0 = düz hareket")]
    public float swayDepth = 0.8f;
    [Tooltip("Yüksekliğin hıza ne kadar çabuk uyduğu (yüksek = sert, düşük = yumuşak)")]
    public float swaySmooth = 6f;

    float baseY;

    [Header("Sınırlar")]
    [Tooltip("Yaprağın gidebileceği en sol X")]
    public float minX = -7.5f;

    [Tooltip("Yaprağın gidebileceği en sağ X")]
    public float maxX = 7.5f;

    [Tooltip("Booster tarafından ayarlanır. 1 = normal hız")]
    [HideInInspector] public float speedMultiplier = 1f;

    [Header("Animasyon")]
    [Tooltip("Yaprağın Animator'ı. Boşsa bu objede ya da child'larında aranır")]
    public Animator animator;
    public string idleState = "Idle";
    public string rightState = "LeafRight";
    public string leftState = "LeafAnim";
    [Tooltip("Bu hızın altında yaprak duruyor sayılır ve Idle oynar")]
    public float moveThreshold = 0.3f;
    [Tooltip("Animasyonlar arası yumuşak geçiş süresi (saniye)")]
    public float animBlend = 0.1f;

    int currentDirection = int.MinValue; // -1 sol, 0 duruyor, 1 sağ

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
        // Dönüşü kilitle. Dikey konumu kod yönetiyor (dalgalı hareket için)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        baseY = rb.position.y;

        if (fans == null || fans.Length == 0)
            fans = FindObjectsByType<Fan>(FindObjectsSortMode.None);

        if (animator == null) animator = GetComponentInChildren<Animator>();
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

        // U şekli: yaprak hızlandıkça aşağı iner, yavaşlayınca tekrar yükselir.
        // Tek basışta: dururken yukarıda → hızlanırken aşağı → dururken yine yukarı.
        float speed01 = Mathf.Clamp01(Mathf.Abs(horizontalVelocity) / maxHorizontalSpeed);
        float targetY = baseY - swayDepth * speed01;
        float verticalVelocity = (targetY - rb.position.y) * swaySmooth;

        rb.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);

        UpdateAnimation();
    }

    // Hareket yönüne göre animasyonu değiştirir. Sadece yön değişince çağrılır, her karede değil.
    void UpdateAnimation()
    {
        if (animator == null) return;

        int direction = 0;
        if (horizontalVelocity > moveThreshold) direction = 1;
        else if (horizontalVelocity < -moveThreshold) direction = -1;

        if (direction == currentDirection) return;
        currentDirection = direction;

        string state = direction == 1 ? rightState : direction == -1 ? leftState : idleState;
        if (!string.IsNullOrEmpty(state))
            animator.CrossFadeInFixedTime(state, animBlend);
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