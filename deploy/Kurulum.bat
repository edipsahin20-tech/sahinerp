@echo off
REM ŞahinSoft Ön Muhasebe - Kurulum başlatıcı
REM Bu dosyaya çift tıklamak, Kurulum.ps1'i Yönetici olarak PowerShell ile çalıştırır.

powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process powershell -Verb RunAs -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File \"%~dp0Kurulum.ps1\"'"
