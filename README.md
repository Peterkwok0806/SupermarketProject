# 🛒 Supermarket E-Commerce Platform

歡迎來到 Supermarket E-Commerce Platform！這是一個功能完整的全端電子商務解決方案，旨在提供流暢、安全且高效的線上購物體驗。前端採用 Angular 打造，後端則由穩定的 ASP.NET Core 驅動，實現了從使用者註冊、商品瀏覽到訂單完成的全流程功能。

## ✨ 特色功能 (Features)

*   **使用者身份驗證**：
    *   完整的註冊、登入與登出流程。
    *   使用 JWT (JSON Web Tokens) 進行安全的 API 驗證。
    *   支援 Refresh Token，可自動刷新 Access Token，提供無縫的使用者體驗。
*   **購物車管理**：
    *   將商品加入、移出購物車。
    *   即時更新購物車內商品數量。
    *   清空購物車。
*   **訂單系統**：
    *   從購物車建立訂單。
    *   查詢歷史訂單列表。
    *   查看單一訂單的詳細資訊。
*   **商品瀏覽與搜尋**：
    *   依分類篩選商品。
    *   透過關鍵字搜尋商品。
    *   提供即時的搜尋建議。
*   **個人資料管理**：
    *   使用者可以更新個人資料。
    *   提供修改密碼功能。
*   **背景任務處理**：
    *   整合 Hangfire 處理背景任務，例如：非同步發送註冊驗證 Email。

## 🛠️ 技術棧 (Tech Stack)

*   **後端 (Backend)**:
    *   .NET 8
    *   ASP.NET Core Web API
    *   Entity Framework Core
    *   SQL Server
    *   Hangfire (用於背景任務)
    *   IdGen (用於產生雪花 ID)
*   **前端 (Frontend)**:
    *   Angular
    *   RxJS
    *   Tailwind CSS

## 🚀 快速開始 (Getting Started)

### 後端 (ASP.NET Core API)

**環境要求**:
*   .NET 8 SDK

**安裝與執行**:
1.  **還原依賴套件**:
    ```bash
    dotnet restore
    ```
2.  **更新資料庫**:
    ```bash
    dotnet ef database update
    ```
3.  **執行專案**:
    ```bash
    dotnet run
    ```
    API 將會在 `https://localhost:7154` 上運行。

### 前端 (Angular)

**環境要求**:
*   Node.js (建議版本 18.x 或更高)
*   Angular CLI

**安裝與執行**:
1.  **安裝依賴套件**:
    ```bash
    npm install
    ```
2.  **啟動開發伺服器**:
    ```bash
    ng serve --ssl
    ```
    前端應用將會在 `https://localhost:4200` 上運行。

## 📁 專案目錄結構 (Project Structure)
.
├── supermarket-app/ # 前端 Angular 專案
│ ├── src/
│ │ ├── app/
│ │ │ ├── components/ # UI 元件 (商品、購物車、訂單等)
│ │ │ ├── models/ # 前端資料模型
│ │ │ └── services/ # API 呼叫與狀態管理服務
│ │ └── ...
│ └── package.json # 前端依賴套件
│
└── SupermarketMock/ # 後端 ASP.NET Core 專案
├── Controllers/ # API 控制器 (Auth, Product, Cart, Order)
├── DTOs/ # 資料傳輸物件
├── Models/ # EF Core 資料模型
├── Services/ # 商業邏輯服務
├── appsettings.json # 應用程式設定
└── Program.cs # 程式進入點與服務設定
