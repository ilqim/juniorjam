using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rastgele aralıklarla sağdan ve soldan dal çıkarır.
// Her yeni dal, karşı taraftaki yakın dallara bakarak konumunu ayarlar:
// aralarındaki süre kısaysa ortada yaprağın geçebileceği kadar boşluk bırakır.
public class BranchSpawner : MonoBehaviour
{
    [Header("Dallar")]
    [Tooltip("5 farklı dal prefabı")]
    public GameObject[] branchPrefabs;

    [Tooltip("Prefablar soldan sağa doğru uzanacak şekilde çizildiyse işaretli bırak. Sağdaki dallar otomatik ters çevrilir.")]
    public bool prefabsFaceRight = true;

    [Header("Zamanlama (saniye)")]
    [Tooltip("Bir çiftin ikinci dalından sonraki çifte kadar bekleme (en az)")]
    public float minInterval = 1.5f;
    [Tooltip("Bir çiftin ikinci dalından sonraki çifte kadar bekleme (en fazla)")]
    public float maxInterval = 3f;

    [Tooltip("İlk taraftan sonra diğer tarafın çıkma gecikmesi (en az)")]
    public float minSideDelay = 0.3f;
    [Tooltip("İlk taraftan sonra diğer tarafın çıkma gecikmesi (en fazla)")]
    public float maxSideDelay = 0.8f;

    [Header("Konum")]
    [Tooltip("Dalın X'i en az bu kadar olabilir (mutlak değer). Sağ: +, sol: -")]
    public float minAbsX = 5f;
    [Tooltip("Dalın X'i en fazla bu kadar olabilir (mutlak değer). Sağ: +, sol: -")]
    public float maxAbsX = 7.5f;

    [Tooltip("Dalların doğduğu Y (ekranın altı)")]
    public float spawnY = -8f;

    [Header("Dallar arası boşluk")]
    [Tooltip("Karşılıklı iki dal aynı yükseklikteyse uçları arasında kalması gereken boşluk")]
    public float minGap = 3f;

    [Tooltip("Karşılıklı yakın iki dalın uçları arasında olabilecek en fazla boşluk. 0 = sınır yok")]
    public float maxGap = 6f;

    [Tooltip("İki dal arasında bu kadar saniye varsa artık birbirini etkilemez. Arada kalan sürelerde gereken boşluk giderek azalır.")]
    public float influenceTime = 1.2f;

    [Header("Hareket")]
    public float scrollSpeed = 2f;
    [Tooltip("Dallar bu Y'yi geçince silinir (ekranın üstü)")]
    public float destroyY = 10f;

    [Header("Başlangıç")]
    public float startDelay = 1f;

    class Branch
    {
        public GameObject go;
        public Scroller scroller;
        public bool isRight;
    }

    // Dalın ekrandaki kapladığı alan. Önce görsele (Renderer), yoksa collider'a bakar.
    static bool TryGetBounds(GameObject go, out Bounds bounds)
    {
        bounds = default;
        bool found = false;

        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            if (!found) { bounds = r.bounds; found = true; }
            else bounds.Encapsulate(r.bounds);
        }

        if (!found)
        {
            foreach (Collider2D c in go.GetComponentsInChildren<Collider2D>())
            {
                if (!found) { bounds = c.bounds; found = true; }
                else bounds.Encapsulate(c.bounds);
            }
        }

        return found;
    }

    readonly List<Branch> branches = new List<Branch>();
    Coroutine loop;

    void Start()
    {
        loop = StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            bool rightFirst = Random.value < 0.5f;

            SpawnBranch(rightFirst);

            yield return new WaitForSeconds(Random.Range(minSideDelay, maxSideDelay));

            SpawnBranch(!rightFirst);

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }

    void SpawnBranch(bool isRight)
    {
        if (branchPrefabs == null || branchPrefabs.Length == 0) return;

        branches.RemoveAll(b => b.go == null);

        float x = Random.Range(minAbsX, maxAbsX) * (isRight ? 1f : -1f);
        GameObject prefab = branchPrefabs[Random.Range(0, branchPrefabs.Length)];
        GameObject go = Instantiate(prefab, new Vector3(x, spawnY, 0f), Quaternion.identity, transform);

        // Sağdaki dal sola doğru uzanmalı: gerekirse ters çevir
        bool shouldFlip = prefabsFaceRight ? isRight : !isRight;
        if (shouldFlip)
        {
            Vector3 s = go.transform.localScale;
            s.x = -Mathf.Abs(s.x);
            go.transform.localScale = s;
        }

        if (go.GetComponentInChildren<Collider>() != null)
        {
            // 3D collider varsa Rigidbody2D eklenemez; uyar ama oyunu durdurma
            Debug.LogError($"{prefab.name} prefabında 3D collider var. Silip yerine Box Collider 2D ekleyin.");
        }
        else
        {
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = go.AddComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Scroller scroller = go.GetComponent<Scroller>();
        if (scroller == null) scroller = go.AddComponent<Scroller>();
        scroller.speed = scrollSpeed;
        scroller.loop = false;
        scroller.destroyWhenOut = true;
        scroller.destroyY = destroyY;

        if (go.GetComponentInChildren<Collider2D>() == null)
            Debug.LogWarning($"{prefab.name} prefabında Collider2D yok, yaprak bu dala çarpınca fail olmaz.");

        Branch branch = new Branch
        {
            go = go,
            scroller = scroller,
            isRight = isRight
        };

        KeepGapFromOpposite(branch);
        branches.Add(branch);
    }

    // Yeni dalı, karşı taraftaki yakın dallara göre gerekirse dışarı iter.
    // Dikey mesafe ne kadar azsa (yani aradaki süre ne kadar kısaysa) o kadar çok boşluk ister.
    void KeepGapFromOpposite(Branch newBranch)
    {
        if (minGap <= 0f && maxGap <= 0f) return;

        if (!TryGetBounds(newBranch.go, out Bounds nb))
        {
            Debug.LogWarning("Dalda Renderer veya Collider2D bulunamadı, boşluk kontrolü yapılamıyor.");
            return;
        }

        float influenceDistance = Mathf.Max(0.01f, influenceTime * scrollSpeed);
        float pushOut = 0f;                       // boşluk az: dışarı itme miktarı
        float pullIn = 0f;                        // boşluk çok: içeri çekme miktarı
        float allowedPull = float.MaxValue;       // min boşluğu bozmadan en fazla ne kadar içeri çekilebilir

        foreach (Branch other in branches)
        {
            if (other.isRight == newBranch.isRight) continue;
            if (!TryGetBounds(other.go, out Bounds ob)) continue;

            // Eski dal yukarıda; aralarındaki dikey boşluk
            float verticalGap = Mathf.Max(0f, ob.min.y - nb.max.y);
            if (verticalGap >= influenceDistance) continue;

            // Aynı hizadaysa minGap, influenceDistance'a yaklaştıkça 0'a iner
            float requiredGap = Mathf.Lerp(minGap, 0f, verticalGap / influenceDistance);

            float horizontalGap = newBranch.isRight
                ? nb.min.x - ob.max.x    // yeni sağda, eski solda
                : ob.min.x - nb.max.x;   // yeni solda, eski sağda

            pushOut = Mathf.Max(pushOut, requiredGap - horizontalGap);

            if (maxGap > 0f)
            {
                pullIn = Mathf.Max(pullIn, horizontalGap - maxGap);
                allowedPull = Mathf.Min(allowedPull, horizontalGap - requiredGap);
            }
        }

        Vector3 outward = newBranch.isRight ? Vector3.right : Vector3.left;

        if (pushOut > 0f)
        {
            // Önce min boşluk: yaprağın geçebilmesi daha önemli
            newBranch.go.transform.position += outward * pushOut;
        }
        else if (pullIn > 0f)
        {
            pullIn = Mathf.Min(pullIn, allowedPull);
            if (pullIn > 0f)
                newBranch.go.transform.position -= outward * pullIn;
        }
    }

    // Fail olunca çağır (Leaf → On Fail): yeni dal çıkmaz, mevcut dallar durur.
    public void StopAll()
    {
        if (loop != null) StopCoroutine(loop);

        branches.RemoveAll(b => b.go == null);
        foreach (Branch b in branches)
            b.scroller.speed = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-maxAbsX, spawnY), new Vector3(-minAbsX, spawnY));
        Gizmos.DrawLine(new Vector3(minAbsX, spawnY), new Vector3(maxAbsX, spawnY));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(-maxAbsX, destroyY), new Vector3(maxAbsX, destroyY));
    }
}