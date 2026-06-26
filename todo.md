# Supermarket Project - Todo List

> 目前進度：準備實作 todo.md 第一個項目 (商品上下架真實開關)

## 🔥 High Priority - 進行中

- [ ] **商品上下架 (IsAvailable) 真實開關**
  - [ ] 後端：IProductService 新增 `ToggleAvailabilityAsync` 方法
  - [ ] 後端：ProductService 實作 `ToggleAvailabilityAsync`
  - [ ] 後端：ProductController 新增 PATCH 路由
  - [ ] 前端：ProductService 新增 `toggleAvailability` API 方法
  - [ ] 前端：AdminProductsComponent 替換 alert 為真實呼叫
  - [ ] 前端：AdminProductsComponent 新增狀態欄 + 即時切換視覺
  - [ ] 編譯測試

## 📋 待辦 (從 todo.md 摘要)

### 高價值
- [ ] 2. 庫存警示 (Low Stock Alert) - Dashboard 卡片 + 後端統計 API
- [ ] 3. 批量操作 (Batch Actions) - checkbox + 批量上下架/刪除
- [ ] 4. 商品刪除 (含圖片清理) - SweetAlert 確認 + 實體檔案清理

### Dashboard 擴充
- [ ] 5. 銷售趨勢圖表 (Chart.js / ng2-charts)
- [ ] 6. 熱銷商品 Top 10
- [ ] 7. 訂單狀態漏斗 (Funnel)

### 前台 / 用戶端
- [ ] 8. 商品評價 & 評論系統
- [ ] 9. 願望清單 / 收藏 (Wishlist)
- [ ] 10. 優惠券系統 (Coupons)
- [ ] 11. 多語系 (i18n)

### 系統 / 工程面
- [ ] 12. 商品匯入 / 匯出 (Excel)
- [ ] 13. 操作日誌 (Audit Log)
- [ ] 14. 圖片管理 / 多圖上傳
- [ ] 15. 通知中心 (SignalR)

### UI / UX
- [ ] 16. 商品頁 SEO 優化
- [ ] 17. 深色模式 (Dark Mode)
- [ ] 18. 骨架屏 (Skeleton Loader)
- [ ] 19. 響應式優化