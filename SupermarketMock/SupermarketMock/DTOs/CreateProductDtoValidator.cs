using FluentValidation;

namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 驗證建立 / 更新商品的 DTO：名稱必填、價格 > 0、庫存 >= 0、分類必選。
    /// </summary>
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("商品名稱不可為空")
                .MaximumLength(200).WithMessage("商品名稱最多 200 字元");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("價格必須大於 0")
                .LessThanOrEqualTo(999999.99m).WithMessage("價格不可超過 999999.99");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("請選擇商品分類");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("庫存量不可為負數");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("商品描述最多 2000 字元")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Brand)
                .MaximumLength(100).WithMessage("品牌名稱最多 100 字元")
                .When(x => !string.IsNullOrEmpty(x.Brand));

            // 圖片驗證（如果有上傳）
            RuleFor(x => x.Photofile)
                .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
                .WithMessage("圖片大小不可超過 5 MB")
                .When(x => x.Photofile != null);
        }
    }
}
