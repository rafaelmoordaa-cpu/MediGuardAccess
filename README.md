# MediGuardAccess

MediGuardAccess adalah aplikasi skrining awal hipertensi berbasis **.NET MAUI** yang dirancang untuk membantu pengguna mencatat tanda vital, mengenali gejala awal, dan memperoleh klasifikasi risiko kesehatan secara terstruktur.

Aplikasi ini mendukung akses berdasarkan peran pengguna, yaitu **Admin** dan **User**, dengan fitur login, registrasi, verifikasi wajah, skrining hipertensi, dashboard monitoring, serta penyimpanan data menggunakan database SQLite.

---

## Tujuan Project

MediGuardAccess dibuat sebagai sistem skrining awal untuk membantu pengguna memahami kondisi kesehatan berdasarkan data tekanan darah, tanda vital, keluhan, dan indikator risiko.

Sistem bukan pengganti diagnosis dokter. Hasil skrining digunakan sebagai rekomendasi awal agar pengguna dapat menentukan langkah lanjutan yang lebih tepat.

---

## Fitur Utama

- Login multi-role Admin dan User
- Registrasi pengguna baru
- Dashboard Admin dan Dashboard User
- Verifikasi wajah / face enrollment
- Skrining awal hipertensi
- Input tekanan darah awal dan pengukuran ulang
- Input SpO₂, denyut nadi, frekuensi napas, dan suhu tubuh
- Input keluhan utama serta red flag
- Klasifikasi hasil skrining:
  - Normal
  - Waspada
  - Berisiko
- Simulasi Fuzzy Logic untuk Symptom Score
- Simulasi Neural Network untuk Risk Score
- Rekomendasi awal berdasarkan hasil skrining
- Riwayat data skrining pengguna
- Database SQLite untuk data login, skrining, dan verifikasi wajah

---

## Teknologi yang Digunakan

- .NET MAUI
- C#
- SQLite
- Visual Studio 2022
- XAML
- Fuzzy Logic Simulation
- Neural Network Risk Simulation
- Face Verification / Face Enrollment Concept

---

## Struktur Database

Project menggunakan database SQLite dengan nama:

```text
MediGuardAccess.sqlite
