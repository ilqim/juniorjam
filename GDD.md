# GDD – JuniorJam (Tema: Fall)

## Özet
Kızılay'daki bir ağaçtan kopan bir yaprağı, iki yelpazeyle sağa sola üfleyerek düşen dallardan kaçırıyorsun. Yaprak ekranda sabit durur; arka plan ve dallar yukarı kayarak düşme hissi verir. Amaç olabildiğince uzun süre dayanmak.

- **Tür:** 2D, skor odaklı hayatta kalma
- **Motor:** Unity (URP 2D)
- **Kontroller:** `A` = sol yelpaze, `D` = sağ yelpaze (basıp bırakarak sallanır)

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
- Üst-ortada yaprak (oyuncu karakteri)
- Sol alt köşede yelpaze (`A`), sağ alt köşede yelpaze (`D`)
- Ekranın sol ve sağ kenarlarından içeri uzanan dallar (engeller)

**Kamera / Hareket:**
- Yaprak dikeyde **sabit**, sadece yatayda hareket eder
- Yatay sınır: `X = -7.5` ile `X = 7.5` arası (Inspector'dan ayarlanabilir)
- Arka plan ve dallar sürekli **yukarı kayar**

**Koşullar:**
- **Win:** Yok (sonsuz oyun)
- **Fail:** Ground'a çarpma *(bkz. Açık Sorular)*
- **Skor:** Süre bazlı, en iyi skor (best score) kaydedilir

---

## Hareket Mekaniği (Yelpaze)

- Tuşa **her basışta** yelpaze **bir kez sallanır** ve yaprağa kısa bir rüzgâr üfler.
- **Ne kadar hızlı basılırsa** üfleme o kadar güçlü olur, yaprak daha hızlı gider.
- Soldaki yelpaze yaprağı **sağa**, sağdaki **sola** iter.
- Güç, yaprağın yelpazeye **uzaklığına** bağlıdır: yakınsa güçlü, uzaksa zayıf.
- Basmayı bırakınca yaprak yavaşça durur.

**Ayarlanabilir değerler (Fan.cs):**

| Değer | Ne yapar |
|-------|----------|
| `baseGust` | Yavaş basışta tek sallamanın gücü |
| `maxRateMultiplier` | En hızlı basışta güç kaç katına çıkar |
| `maxTapsPerSecond` | Saniyede kaç basış en yüksek güç sayılır |
| `gustDuration` | Bir sallamanın rüzgârı ne kadar sürer |
| `maxDistance` / `farMultiplier` | Menzil ve uzakta kalan güç oranı |

---

## Engeller (Dallar)

- **5 farklı dal**, hepsi **aynı uzunlukta**
- Ekranın sol veya sağ kenarından içeri uzanırlar
- Aşağıdan gelip yukarı doğru kayarlar

---

## Gerekenler

### Görsel
- [ ] Karakter (yaprak) tasarımı
- [ ] Game background
- [ ] 5 farklı dal (aynı uzunlukta)
- [ ] UI (menü, skor, play again)
- [ ] Yelpaze (sallanma animasyonuyla)
- [ ] Rüzgâr efekti (particle effect olabilir)
- [ ] Cutscene görselleri

### Ses
- [ ] Menü müziği
- [ ] Oyun içi müzik
- [ ] Fail müziği
- [ ] Buton sesi
- [ ] Yaprak ezilme sesi (fail)
- [ ] Yelpaze sesi

---

## Kodlar

| Script | Nereye | Görevi |
|--------|--------|--------|
| `Leaf.cs` | Yaprak | Yelpazelerden gelen gücü toplar, yatayda hareket eder, X sınırı, fail kontrolü |
| `Fan.cs` | Her iki yelpaze | Basış hızına ve uzaklığa göre rüzgâr üretir |
| `Scroller.cs` | Arka plan ve dallar | Objeyi yukarı kaydırır, arka planı döngüye sokar |

---

## Açık Sorular
- Yaprak artık düşmediği için ground'a çarpamaz. Fail **dala çarpmak** mı olacak?
- Dallar rastgele mi gelecek, yoksa elle tasarlanmış bir sırayla mı?
- Zaman geçtikçe zorluk artıyor mu? (kayma hızının artması, dalların sıklaşması vb.)