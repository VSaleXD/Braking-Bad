# Braking Bad Setup Guide

Panduan ini menjelaskan cara membangun game party multiplayer 2D top-down **Braking Bad** di Unity, dari setup project, tilemap, scene, GameObject, prefab, sampai alur tournament.

## 1. Gambaran Besar

Struktur game yang dipakai:

- 1 scene bootstrap/menu
- 10 scene minigame
- 1 scene podium akhir
- 4 prefab player vehicle
- 1 TournamentManager persisten
- 1 manager minigame per scene
- UI Toolkit untuk score dan timer

Alur permainan:

1. Game dimulai dari bootstrap scene.
2. TournamentManager memilih 3 minigame secara acak dari pool 10 scene.
3. Setiap minigame berjalan 90 detik.
4. Saat timer habis, skor minigame diubah menjadi urutan placement.
5. Placement dikonversi menjadi Tournament Points.
6. Setelah 3 match, game pindah ke FinalPodiumScene.

---

## 2. File Skrip Utama

Skrip inti yang sudah ada:

- [Assets/Scripts/Tournament/PlayerMatchResult.cs](Assets/Scripts/Tournament/PlayerMatchResult.cs)
- [Assets/Scripts/Tournament/TournamentManager.cs](Assets/Scripts/Tournament/TournamentManager.cs)
- [Assets/Scripts/Tournament/BaseMinigameManager.cs](Assets/Scripts/Tournament/BaseMinigameManager.cs)
- [Assets/Scripts/Tournament/TournamentPlayerAgent.cs](Assets/Scripts/Tournament/TournamentPlayerAgent.cs)
- [Assets/Scripts/playerController.cs](Assets/Scripts/playerController.cs)

Skrip minigame:

- [Assets/Scripts/Tournament/Minigames/Minigame_CarSoccer.cs](Assets/Scripts/Tournament/Minigames/Minigame_CarSoccer.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_DriftMadness.cs](Assets/Scripts/Tournament/Minigames/Minigame_DriftMadness.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_ObstacleSurvival.cs](Assets/Scripts/Tournament/Minigames/Minigame_ObstacleSurvival.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_CarSumo.cs](Assets/Scripts/Tournament/Minigames/Minigame_CarSumo.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_ChaseTheUFO.cs](Assets/Scripts/Tournament/Minigames/Minigame_ChaseTheUFO.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_PortalRush.cs](Assets/Scripts/Tournament/Minigames/Minigame_PortalRush.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_FloorIsLava.cs](Assets/Scripts/Tournament/Minigames/Minigame_FloorIsLava.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_Spotlight.cs](Assets/Scripts/Tournament/Minigames/Minigame_Spotlight.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_CaptureTheFlag.cs](Assets/Scripts/Tournament/Minigames/Minigame_CaptureTheFlag.cs)
- [Assets/Scripts/Tournament/Minigames/Minigame_MirrorDimension.cs](Assets/Scripts/Tournament/Minigames/Minigame_MirrorDimension.cs)

---

## 3. Struktur Folder yang Disarankan

Buat folder seperti ini di `Assets`:

- `Assets/Scenes`
- `Assets/Scenes/Bootstrap`
- `Assets/Scenes/Minigames`
- `Assets/Scenes/Final`
- `Assets/Prefabs`
- `Assets/Prefabs/Players`
- `Assets/Prefabs/Minigames`
- `Assets/Tilemaps`
- `Assets/UI`
- `Assets/UI/UIDocuments`
- `Assets/Materials`
- `Assets/Scripts/Tournament`
- `Assets/Scripts/Tournament/Minigames`

Folder ini tidak wajib, tapi sangat membantu supaya scene dan prefab tidak berantakan.

---

## 4. Workflow Scene Template

Scene Template dipakai sebagai cetakan untuk membuat scene minigame agar struktur awal tiap mode konsisten.

### Cara pakai yang disarankan

1. Buat satu scene minigame contoh dengan isi paling lengkap.
2. Simpan scene itu sebagai Scene Template.
3. Gunakan Scene Template untuk membuat 10 scene minigame terpisah.
4. Rename hasil scene sesuai nama yang dipakai di `minigamePool`.
5. Simpan tiap hasilnya sebagai file `.unity` biasa, karena yang diload saat runtime tetap scene Unity, bukan file template.

### Isi dasar yang sebaiknya ada di Scene Template

- `Main Camera`
- `UIDocument` atau UI root
- `GameManager` minigame
- `Grid` atau arena dasar
- `SpawnPoints`
- objek khusus mode, misalnya goal, checkpoint, portal, flag, hazard, atau boundary

### Yang sebaiknya tidak ikut Scene Template minigame

- `TournamentManager`
- objek bootstrap/menu
- logic global yang harus persisten antar scene

### Catatan penting

Scene Template hanya membantu membuat scene baru lebih cepat. Saat game jalan, `TournamentManager` tetap akan me-load scene hasil akhirnya lewat nama scene biasa.

---

## 5. Setup Build Settings

Masukkan scene berikut ke Build Settings dalam urutan yang jelas:

1. `BootstrapScene` atau `MenuScene`
2. 10 scene minigame
3. `FinalPodiumScene`

Pastikan nama scene di Build Settings sama persis dengan isi `minigamePool` di `TournamentManager`.

Contoh nama scene minigame:

- `Minigame_CarSoccer`
- `Minigame_DriftMadness`
- `Minigame_ObstacleSurvival`
- `Minigame_CarSumo`
- `Minigame_ChaseTheUFO`
- `Minigame_PortalRush`
- `Minigame_FloorIsLava`
- `Minigame_Spotlight`
- `Minigame_CaptureTheFlag`
- `Minigame_MirrorDimension`

---

## 6. Bootstrap Scene

Bootstrap scene adalah titik awal game.

### GameObject wajib

Buat GameObject kosong bernama `AppRoot` atau `GameRoot`.

Pasang komponen:

- `TournamentManager`

Jika memakai menu UI:

- `Canvas` atau `UIDocument`
- tombol Start
- tombol Quit

### Checklist Bootstrap Scene

- [ ] Buat scene bootstrap/menu
- [ ] Tambahkan `AppRoot`
- [ ] Pasang `TournamentManager`
- [ ] Isi `minigamePool` dengan 10 nama scene
- [ ] Set `finalPodiumSceneName` ke `FinalPodiumScene`
- [ ] Tambahkan UI menu jika diperlukan
- [ ] Hubungkan tombol Start ke `TournamentManager.BeginTournament()`
- [ ] Masukkan scene ini ke Build Settings

### Urutan kerja yang paling enak

1. Buat satu scene contoh minigame dulu.
2. Jadikan itu Scene Template.
3. Generate 10 scene minigame dari template.
4. Baru isi tiap scene dengan objek spesifik mode.
5. Setelah semua siap, lengkapi bootstrap scene dan hubungkan ke TournamentManager.

### Alur yang terjadi

Saat tombol Start ditekan:

- `TournamentManager` memilih 3 scene secara acak
- scene pertama minigame diload
- `TournamentManager` tetap hidup karena `DontDestroyOnLoad`

---

## 7. Prefab Player Vehicle

Setiap pemain harus memakai prefab mobil yang sama atau variasi berbeda.

### Komponen yang wajib ada

- `SpriteRenderer` atau visual model mobil
- `Rigidbody2D`
- `Collider2D`
- `playerController`
- `TournamentPlayerAgent`
- `TrailRenderer` untuk efek drift

### Nilai penting di `TournamentPlayerAgent`

- `playerID`: 1, 2, 3, 4
- `teamIndex`: digunakan untuk mode tim
- `steeringMultiplier`: default 1
- `throttleMultiplier`: default 1

### Nilai penting di `playerController`

- `thrustforce`: dorongan maju
- `maxSpeed`: batas kecepatan
- `rotaionSpeed`: kecepatan putar
- `driftFactor`: semakin kecil, drift semakin terasa
- `driftSteerLag`: mengatur keterlambatan belok saat speed tinggi
- `wallBounceMultiplier`: seberapa kuat pantulan dinding
- `wallBounceMaterial`: material fisika bouncy jika dipakai

### Checklist Player Prefab

- [ ] Pasang `Rigidbody2D`
- [ ] Pasang `Collider2D`
- [ ] Pasang `playerController`
- [ ] Pasang `TournamentPlayerAgent`
- [ ] Set `playerID` unik untuk tiap pemain
- [ ] Set `teamIndex` bila mode butuh tim
- [ ] Tambahkan trail ban jika diinginkan
- [ ] Pastikan prefab bisa bergerak dan berputar di scene test

---

## 8. UI Toolkit Setup

`BaseMinigameManager` mencari label berikut:

- `ScoreText`
- `TimerText`
- `ComboText`

### Langkah membuat UI Toolkit

1. Buat `UI Document` di scene minigame.
2. Siapkan `Panel Settings`.
3. Buat UXML dengan label bernama `ScoreText`.
4. Buat label bernama `TimerText`.
5. Buat label bernama `ComboText` jika ingin pesan combo.

### Contoh isi UI minimum

- score di kiri atas
- timer di kanan atas
- combo message di tengah atas atau tengah layar

### Checklist UI

- [ ] Buat `UIDocument`
- [ ] Hubungkan `PanelSettings`
- [ ] Pastikan ada `ScoreText`
- [ ] Pastikan ada `TimerText`
- [ ] Tambahkan `ComboText` jika diperlukan

---

## 9. Tilemap Setup

Untuk minigame yang memakai arena berbasis grid, seperti `FloorIsLava`, gunakan Tilemap.

### Cara membuat Tilemap

1. Buat `Grid`.
2. Di bawah Grid, buat `Tilemap`.
3. Tambahkan `Tilemap Renderer`.
4. Tambahkan `Tilemap Collider 2D` jika tile perlu collision.
5. Jika ingin kolisi digabung, tambahkan `Composite Collider 2D` dan `Rigidbody2D` pada Grid.

### Untuk Floor Is Lava

Ada dua pendekatan:

#### Opsi A: Tile per GameObject

- Buat prefab tile satu per satu.
- Pasang `FloorIsLavaTile`.
- Pasang `SpriteRenderer`.
- Pasang `Collider2D`.
- Set collider sebagai trigger jika dibutuhkan.

#### Opsi B: Tilemap + relay

- Pakai Tilemap sebagai visual.
- Tambahkan GameObject trigger di atas tile tertentu.
- Tiap trigger punya `FloorIsLavaTile`.

### Rekomendasi

Kalau ingin implementasi cepat, pakai **Opsi A**.
Kalau ingin arena yang besar dan rapi, pakai **Opsi B**.

### Checklist Tilemap

- [ ] Buat `Grid`
- [ ] Buat `Tilemap`
- [ ] Pasang `Tilemap Renderer`
- [ ] Pasang collider sesuai kebutuhan
- [ ] Tentukan apakah tile dibuat sebagai prefab atau relay trigger
- [ ] Pastikan player bisa mendeteksi tile

---

## 10. Scene Minigame: Struktur Umum

Setiap scene minigame minimal punya:

1. Satu `BaseMinigameManager` turunan sesuai mode.
2. UI Toolkit untuk score dan timer.
3. Arena / map.
4. Player spawn point.
5. Objek gameplay khusus mode tersebut.

### GameObject umum yang biasanya ada

- `Main Camera`
- `GameManager`
- `UI Document`
- `SpawnPoints`
- `Arena`
- `Obstacle` / `Goal` / `Checkpoint` / `Portal` / `Flag` tergantung mode

---

## 11. Checklist Per Scene Minigame

### 10.1 Car Soccer

GameObject yang perlu ada:

- `Minigame_CarSoccer` manager
- bola dengan `Rigidbody2D`, `Collider2D`, tag `Ball`
- gawang kiri dengan `CarSoccerGoalTrigger`
- gawang kanan dengan `CarSoccerGoalTrigger`
- arena sepak bola
- spawn point player

Checklist:

- [ ] Buat bola dan beri tag `Ball`
- [ ] Pasang collider trigger di gawang
- [ ] Set `scoringTeamIndex` pada tiap gawang
- [ ] Pasang manager minigame

### 10.2 Drift Madness

GameObject yang perlu ada:

- `Minigame_DriftMadness` manager
- checkpoint trigger
- track / lintasan
- spawn point start

Checklist:

- [ ] Buat checkpoint berurutan
- [ ] Pasang `DriftMadnessCheckpointTrigger`
- [ ] Isi `checkpointIndex`
- [ ] Pastikan urutan checkpoint konsisten

### 10.3 Obstacle Survival

GameObject yang perlu ada:

- `Minigame_ObstacleSurvival` manager
- spawn point hazard
- prefab hazard dengan `ObstacleHazard`
- arena survival

Checklist:

- [ ] Buat prefab hazard
- [ ] Pasang `ObstacleHazard`
- [ ] Isi `hazardPrefabs` pada manager
- [ ] Isi `spawnPoints`

### 10.4 Car Sumo

GameObject yang perlu ada:

- `Minigame_CarSumo` manager
- boundary arena dengan trigger collider
- arena lingkaran atau kotak

Checklist:

- [ ] Buat boundary trigger besar
- [ ] Pasang `CarSumoArenaBoundaryTrigger`
- [ ] Pastikan car keluar boundary terdeteksi

### 10.5 Chase The UFO

GameObject yang perlu ada:

- `Minigame_ChaseTheUFO` manager
- prefab UFO
- arena terbuka

Checklist:

- [ ] Buat prefab UFO
- [ ] Pasang `Rigidbody2D`
- [ ] Pasang `Collider2D`
- [ ] Pasang `ChaseTheUFOActor`
- [ ] Isi `ufoPrefab` pada manager

### 10.6 Portal Rush

GameObject yang perlu ada:

- `Minigame_PortalRush` manager
- portal entrance trigger
- destination transform
- arena lintasan

Checklist:

- [ ] Buat pasangan portal
- [ ] Pasang `PortalRushTeleportTrigger`
- [ ] Isi destination
- [ ] Set sequence index jika portal berurutan

### 10.7 Floor Is Lava

GameObject yang perlu ada:

- `Minigame_FloorIsLava` manager
- grid tile
- tile prefab dengan `FloorIsLavaTile`
- collider tile

Checklist:

- [ ] Buat tile floor
- [ ] Pasang `FloorIsLavaTile`
- [ ] Pasang `SpriteRenderer`
- [ ] Pasang `Collider2D`
- [ ] Atur delay crack dan collapse

### 10.8 Spotlight

GameObject yang perlu ada:

- `Minigame_Spotlight` manager
- `Light2D` ambient
- `Light2D` target
- aura visual prefab jika perlu
- arena gelap

Checklist:

- [ ] Pasang lighting 2D
- [ ] Pastikan target bisa berpindah
- [ ] Tambahkan relay kontak jika ingin bump target

### 10.9 Capture The Flag

GameObject yang perlu ada:

- `Minigame_CaptureTheFlag` manager
- prefab flag dengan `CaptureTheFlagItem`
- trigger base tim dengan `CaptureTheFlagBaseTrigger`
- arena tim
- spawn point flag

Checklist:

- [ ] Buat prefab flag
- [ ] Pasang `Rigidbody2D` pada flag jika perlu fisik
- [ ] Pasang `CaptureTheFlagItem`
- [ ] Buat base trigger tim
- [ ] Set `teamIndex` untuk tiap base

### 10.10 Mirror Dimension

GameObject yang perlu ada:

- `Minigame_MirrorDimension` manager
- checkpoint seperti Drift Madness
- track yang sama atau mirip Drift Madness

Checklist:

- [ ] Gunakan layout track yang sama dengan Drift Madness
- [ ] Pasang checkpoint trigger yang sama
- [ ] Pastikan player punya `TournamentPlayerAgent`
- [ ] Steering akan dibalik otomatis

---

## 12. Urutan Kerja yang Paling Aman

1. Selesaikan bootstrap scene.
2. Buat prefab player.
3. Buat UI Toolkit dasar.
4. Buat satu minigame dulu, misalnya Car Soccer.
5. Test minigame itu sampai jalan.
6. Duplikasi pola untuk mode lain.
7. Baru buat sisa 9 scene minigame.
8. Masukkan semua scene ke Build Settings.
9. Test tournament penuh dari bootstrap ke podium.

---

## 13. Tuning Gameplay yang Disarankan

### Drifting

Jika drift kurang terasa:

- turunkan `driftFactor`
- turunkan `driftSteerLag`
- naikkan `maxSpeed` sedikit
- perbesar perbedaan antara grip lurus dan grip saat belok

### Bounce dinding

Jika pantulan terlalu lemah:

- naikkan `wallBounceMultiplier`
- pastikan collider dinding benar-benar memantul
- gunakan physics material bouncy jika diperlukan

### Skor minigame

Pastikan setiap mode punya definisi skor yang jelas:

- Car Soccer: goal = poin besar
- Drift Madness: checkpoint + speed bonus
- Obstacle Survival: poin per detik bertahan
- Car Sumo: urutan eliminasi
- UFO Chase: tiap tabrakan dengan UFO
- Portal Rush: urutan portal / arrival sequence
- Floor Is Lava: poin bertahan + bonus tile
- Spotlight: target score per detik
- Capture The Flag: capture bonus besar
- Mirror Dimension: sama seperti Drift Madness, tapi steering terbalik

---

## 14. Troubleshooting

### Scene tidak pindah

Cek:

- nama scene di `minigamePool`
- scene sudah masuk Build Settings
- `TournamentManager` benar-benar ada di scene awal

### Score tidak tampil

Cek:

- `UIDocument` ada di scene
- label bernama `ScoreText`
- label bernama `TimerText`
- `uiDocument` tersambung

### Player tidak bergerak

Cek:

- `Rigidbody2D` ada
- `playerController` ada
- mouse input tersedia
- camera aktif

### Drift terasa aneh

Cek:

- `driftFactor`
- `driftSteerLag`
- `maxSpeed`
- collider dan mass Rigidbody2D

### Flag tidak bisa diambil

Cek:

- flag punya `CaptureTheFlagItem`
- player punya `TournamentPlayerAgent`
- collider flag dan player saling bertabrakan

---

## 15. Minimum Viable Build

Kalau ingin cepat test dulu, buat urutan minimal ini:

1. Bootstrap scene dengan `TournamentManager`
2. 1 prefab player
3. 1 minigame saja, misalnya Car Soccer
4. UI Toolkit dasar
5. Build Settings sudah berisi bootstrap, minigame, podium

Setelah itu baru perluas ke 10 minigame lengkap.

---

## 16. Catatan Penting

- Nama scene harus konsisten.
- Semua player harus punya ID unik 1 sampai 4.
- Jangan menaruh lebih dari satu `TournamentManager` aktif.
- Kalau scene minigame tidak punya UI Toolkit, game tetap jalan, tapi score/timer tidak tampil.
- Gunakan prefab dan trigger yang jelas supaya setup antar scene konsisten.

---

## 17. Next Step

Langkah berikutnya yang paling berguna adalah:

- membuat checklist per scene dalam bentuk tabel,
- atau membuat template hierarchy Unity untuk tiap scene.

