using UnityEngine;

// Objeyi sürekli yukarı kaydırır; yaprak sabitken düşüyormuş hissi verir.
// Arka plan için "loop" açılırsa belli bir yüksekliğe gelince başa döner (sonsuz arka plan).
public class Scroller : MonoBehaviour
{
    [Tooltip("Yukarı kayma hızı (yaprağın 'düşme' hızı gibi düşün)")]
    public float speed = 2f;

    [Header("Arka plan için")]
    [Tooltip("Açıksa obje loopHeight kadar kayınca başlangıç konumuna döner")]
    public bool loop = false;

    [Tooltip("Arka plan görselinin yüksekliği (Unity birimi)")]
    public float loopHeight = 10f;

    [Header("Engeller için")]
    [Tooltip("Açıksa obje bu Y değerini geçince yok edilir")]
    public bool destroyWhenOut = false;
    public float destroyY = 15f;

    Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        if (loop && transform.position.y - startPosition.y >= loopHeight)
            transform.position = startPosition;

        if (destroyWhenOut && transform.position.y > destroyY)
            Destroy(gameObject);
    }
}