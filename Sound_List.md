# 🎵 Achilles — Ses Listesi

Mevcut sesler + eklenebilecek sesler kategorize edilmiş tam liste.

---

## ✅ MEVCUT SESLER (27)

```
Theme
Hover
Click
Walk
Jump
Attack
Fall
Throw
Hurt
Death
BlockHit
Block
JavelinHitEnemy
JavelinHitWall
SkeletonDeath
SkeletonHurt
SkeletonAttack
PlayerHeal
PlayerShieldUp
PlayerMaxHpUp
BatHurt
ShieldBreak
LeverPull
SatyrAttack
SatyrHurt
SatyrDeath
PlatformCrumble
```

---

## ❌ ÖNCELİK 1 — KRİTİK EKSİKLER (15 ses)

Kod bunları çağırıyor ama henüz SoundManager'da yok. Eklemen halinde anında çalışır.

### Düşmanlar (5)
| ID | Olay |
|---|---|
| `BatDeath` | Bat ölünce |
| `BatContactHit` | Bat temas hasarı verince |
| `SkeletonContactHit` | Skeleton temas hasarı verince |
| `SatyrContactHit` | Satyr temas hasarı verince |
| `EnemyProjectileHit` | Düşman projectile (ok/büyü) çarpınca |

### Collectibles (4)
| ID | Olay |
|---|---|
| `PickupShield` | Kalkan toplama |
| `PickupHealthSmall` | Küçük can toplama |
| `PickupHealthFull` | Tam can toplama |
| `PickupMaxHealth` | Max HP artırıcı (özel/zafer tonu) |

### Level objeleri (4)
| ID | Olay |
|---|---|
| `SpikeHit` | Dikene değme (ani ölüm) |
| `ElevatorMove` | Asansör başlama |
| `ElevatorStop` | Asansör durma |
| `LevelComplete` | Level bitirme (kapıya değme) |

### Platform & Boss UI (3)
| ID | Olay |
|---|---|
| `PlatformRespawn` | Kaybolan platform geri gelme |
| `BossRoar` | Boss aktive olunca kükreme |
| `BossUIAppear` | HP bar belirme (whoosh) |
| `BossUIDisappear` | HP bar kaybolma |

---

## 🎼 ÖNCELİK 2 — MÜZİKLER

Şu an sadece `Theme` (ana menü) var.

### Level müzikleri (her birine ayrı veya tema bazlı paylaşımlı)
**Opsiyon A — Her level için ayrı:**
```
Level1Music
Level2Music
Level3Music
Level4Music
Level5Music
Level6Music
```

**Opsiyon B — Temaya göre paylaşımlı (önerilen, daha az clip):**
```
AmbientCalm        ← Sakin keşif bölümleri
AmbientTension     ← Düşman yoğun bölümler
AmbientHell        ← Cehennem temalı bölümler (Hell tileset için)
```

### Boss müzikleri
```
CerberusMusic      ← Loop, Music mixer
ZagreusMusic       ← Loop, Music mixer
```

### Geçiş/UI müzikleri
```
VictoryMusic       ← Boss/level bitirince (kısa, loop yok)
GameOverMusic      ← Death screen müziği
PauseMusic         ← Pause açıkken (opsiyonel)
LevelTransition    ← Sahne geçiş whoosh (loop yok)
```

> **Not:** Loop olması gereken müziklerde `Loop ✅` işaretle, **Mixer Group: Music** ata.

---

## 👹 ÖNCELİK 3 — BOSS SESLERİ

### Cerberus (~7 ses)
BossController inspector'da bu isimleri girersin:
```
CerberusIntro          ← Aktivasyon kükremesi
CerberusBite           ← Melee saldırı (ısırma)
CerberusFireBall       ← Ranged ateş topu fırlatma
CerberusFireBallHit    ← Ateş topu impact (duvar/player)
CerberusHurt           ← Hasar
CerberusDeath          ← Ölüm
CerberusFootstep       ← Yürüme adımı (opsiyonel)
```

### Zagreus (~10 ses)
ZagreusController inspector'da bu isimleri girersin:
```
ZagreusActivate        ← Aktivasyon
ZagreusAttack1         ← Solo Attack 1
ZagreusAttack2         ← Solo Attack 2
ZagreusAttack3         ← Standalone Attack 3
ZagreusChainCombo      ← Chain combo başı (whoosh)
ZagreusChainCombo2     ← Chain combo 2. vuruş impact
ZagreusBackDash        ← Geri sıçrama (swoosh)
ZagreusHurt            ← Hasar
ZagreusDeath           ← Ölüm
ZagreusFootstep        ← Yürüme (opsiyonel)
```

---

## ✨ ÖNCELİK 4 — POLISH (opsiyonel ama oyunu canlandırır)

### Ortam ambient'i (loop, düşük volume)
```
AmbientWind            ← Açık alan rüzgar
AmbientCave            ← Mağara/kapalı alan
AmbientFire            ← Mum/meşale çıtırtısı
AmbientWaterDrip       ← Su damlası
```

### Footstep varyasyonları (immersion)
```
WalkStone              ← Taş zemin
WalkGrass              ← Çim
WalkMetal              ← Metal platform
WalkWood               ← Tahta
```

### Combat polish (varyasyon)
```
SwordSwingLight        ← Hafif kılıç sallama (boş alan)
SwordSwingHeavy        ← Ağır
HitFlesh               ← Etkili vuruş hit
HitMetal               ← Zırha vuruş
DodgeWhoosh            ← Hızlı kaçınma (opsiyonel mekanik)
```

### UI polish
```
MenuOpen               ← Pause/menü açılış
MenuClose              ← Kapanış
TabSwitch              ← Sekme değişimi
ErrorBeep              ← Geçersiz aksiyon
```

---

## 📊 ÖZET

| Kategori | Sayı | Durum |
|---|---|---|
| Mevcut sesler | 27 | ✅ |
| Kritik eksikler | 15 | ❌ |
| Müzikler | 6-12 | ❌ |
| Boss sesleri | ~17 | ❌ |
| Polish | ~17 | ⏸️ Opsiyonel |
| **Minimum hedef (kritik + minimal müzik+boss)** | **~25-28** | |
| **Tam set (polish dahil)** | **~75** | |

---

## 🎯 PRATİK YOL HARİTASI

### Aşama 1 — Minimum oynanabilir set (~25 ses)
1. 15 kritik eksiği ekle (oyun mekanikleri tam tepki versin)
2. 1 boss müziği + 1-2 level müziği
3. Cerberus için 4 ana ses (Bite, FireBall, Hurt, Death)
4. Zagreus için 4 ana ses (Attack1, Attack2, Attack3, ChainCombo)

### Aşama 2 — Tam set
5. Geri kalan boss sesleri (intro, footstep vb.)
6. Geçiş/UI müzikleri (Victory, GameOver, Transition)
7. Polish: ambient + footstep varyasyonu

### Aşama 3 — Final touch
8. Combat varyasyonları (SwordSwing, Hit*)
9. UI polish (MenuOpen/Close)

---

## ⚙️ SOUND MANAGER AYARI HATIRLATMASI

Her ses için SoundManager'da:
- **Name**: yukarıdaki ID birebir (case-sensitive)
- **Clip**: ses dosyası
- **Volume**: 0.6-0.8 arası genelde
- **Pitch**: 1.0 (varsayılan)
- **Loop**: ✅ sadece müzik ve ambient için, SFX için ❌
- **Mixer Group**:
  - Music → arka plan müziği + ambient loop
  - SFX → tek-atış efektler

> 💡 İpucu: `TryPlayOneShot` kullanıldığı için eksik bırakılan ID'ler **sessizce atlanır**, hata vermez. Aşamalı ekleyebilirsin.
