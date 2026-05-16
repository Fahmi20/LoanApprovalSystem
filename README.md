# Database Migration Guide

Dokumen ini menjelaskan cara membuat dan menjalankan database untuk project Loan Approval System menggunakan Entity Framework Core Migration.

## Database

Project ini menggunakan:

- SQL Server
- Entity Framework Core
- ASP.NET Identity
- Migration bawaan EF Core

Nama database yang digunakan:
LoanApprovalDB

# appsettings.json

menggunakan Windows Authentication:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=LoanApprovalDB;Trusted_Connection=True;TrustServerCertificate=True;"
}

menggunakan SQL Server Authentication:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=LoanApprovalDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}

lalu Jalankan 
Package Manager Console

[POWER SHELL]
Update-Database


# Loan Approval System

Loan Approval System adalah aplikasi web berbasis ASP.NET Core MVC untuk proses pengajuan peminjaman dengan alur approval berdasarkan role pengguna.

Project ini dibuat untuk kebutuhan User Test posisi .NET Developer.

## Teknologi yang Digunakan

- ASP.NET Core MVC (.NET 10)
- SQL Server
- Entity Framework Core
- ASP.NET Identity
- Bootstrap
- Google reCAPTCHA
- Upload file PDF

## Fitur Utama

- Login dan Register User
- Register user dengan pilihan role:
  - Admin
  - Staff
  - Manager
  - Direktur
- Role-based access
- Pengajuan peminjaman oleh Staff
- Upload attachment PDF pada form pengajuan
- Generate nomor peminjaman otomatis
- Approval Manager
- Approval Direktur
- Reject pengajuan oleh Manager dan Direktur
- Tracking status pengajuan
- Approver dapat melihat dokumen PDF yang di-upload

## Alur Approval

1. Staff membuat pengajuan peminjaman.
2. Status awal pengajuan menjadi `Pending Manager`.
3. Manager melakukan approval.
4. Jika nominal pengajuan kurang dari Rp 10.000.000, status menjadi `Approved`.
5. Jika nominal pengajuan Rp 10.000.000 atau lebih, status menjadi `Pending Direktur`.
6. Direktur melakukan approval.
7. Status akhir menjadi `Approved` atau `Rejected`.

## Ketentuan Approval

- Nominal < Rp 10.000.000  
  Approval hanya sampai Manager.

- Nominal >= Rp 10.000.000  
  Approval melalui Manager lalu Direktur.

## Role Access

### Staff

- Membuat pengajuan peminjaman
- Upload dokumen PDF
- Melihat tracking status pengajuan miliknya

### Manager

- Melihat pengajuan yang menunggu approval Manager
- Melihat dokumen PDF
- Approve atau Reject pengajuan
- Jika nominal >= Rp 10.000.000, pengajuan diteruskan ke Direktur

### Direktur

- Melihat pengajuan yang menunggu approval Direktur
- Melihat dokumen PDF
- Approve atau Reject pengajuan

### Admin

- Dapat mengelola data secara umum
