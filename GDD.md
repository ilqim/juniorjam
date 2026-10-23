# GDD – JuniorJam (Tema: Fall)

## Özet
Kızılay'daki bir ağaçtan kopan bir yaprağı, iki fanın rüzgârıyla havada tutmaya çalışıyorsun. Yaprak sürekli aşağı düşer; amaç yere çarpmadan olabildiğince uzun süre dayanmak.

- **Tür:** 2D, skor odaklı hayatta kalma
- **Motor:** Unity (URP 2D)
- **Kontroller:** `A` = sol fan, `D` = sağ fan

---

## Oyun Akışı

| # | Sahne | İçerik |
|---|-------|--------|
| 1 | Main Menu | Oyuna başla |
| 2 | Cutscene 1 | Kızılay'da bir ağaç, etrafta insanlar yürüyor |
| 3 | Cutscene 2 | Karaktere (yaprağa) yakın çekim, yapraklar mutlu |
| 4 | Cutscene 3 | Cutscene 2'nin kopyası, dal kopuyor |
| 5 | Cutscene 4 | Yaprak aşağı düşüyor, korku |
| 6 | Game Scene | Oynanış (aşağıda) |
| 7 | Fail Cutscene | Yaprak ayakla eziliyor + ses |
| 8 | Play Again Canvas | **Play Again** ve **Main Menu** butonları |

---

## Game Scene

**Yerleşim:**
- Ortada düşen yaprak (oyuncu karakteri)
- Sol alt köşede fan (`A` tuşu), sağ alt köşede fan (`D` tuşu)
- Sahnede yaprağın çarpabileceği platformlar/engeller

**Koşullar:**
- **Win:** Yok (sonsuz oyun)
- **Fail:** Yaprağın zemine (ground) çarpması
- **Skor:** Süre bazlı, en iyi skor (best score) kaydedilir

---

## Hareket Mekaniği

Yaprağa her frame iki kuvvet etki eder:

1. **Gravity vektörü:** Her zaman aşağı doğru.
2. **Hareket vektörü:** Basılı tutulan fanın **tersi yönünde**. Şiddeti, yaprağın fana olan **uzaklığına** bağlı (yakınsa güçlü, uzaksa zayıf).

**Kural:** Her zaman `gravity.y > hareket.y` olmalı. Yani fanlar yaprağı yavaşlatır ve yana iter ama asla yukarı kaldıramaz; yaprak her durumda aşağı iner.

```
   ↖ hareket
   |
   ↓ gravity   (gravity her zaman baskın)
```

---

## Gerekenler

### Görsel
- [ ] Karakter (yaprak) tasarımı
- [ ] Game background
- [ ] Yaprağın çarpacağı engeller/platformlar
- [ ] UI (menü, skor, play again)
- [ ] Fan
- [ ] Fan rüzgârı (particle effect olabilir)
- [ ] Cutscene görselleri

### Ses
- [ ] Menü müziği
- [ ] Oyun içi müzik
- [ ] Fail müziği
- [ ] Buton sesi
- [ ] Yaprak ezilme sesi (fail)
- [ ] Fan sesi

---

## Açık Sorular
- Engellere çarpınca ne oluyor? (sekme, yavaşlama, fail?)
- Kamera yaprakla birlikte aşağı mı kayıyor, yoksa sahne sabit mi?
- Zaman geçtikçe zorluk artıyor mu? (gravity artışı, daha çok engel vb.)
