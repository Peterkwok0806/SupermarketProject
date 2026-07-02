using FluentValidation;

namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 驗證建立訂單的 DTO：收貨人姓名、電話、地址皆為必填。
    /// </summary>
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.FullName)
                .RequiredName();

            RuleFor(x => x.Phone)
                .PhoneNumber();

            RuleFor(x => x.Address)
                .RequiredAddress();

            RuleFor(x => x.Remark)
                .MaximumLength(1000).WithMessage("備註最多 1000 字元")
                .When(x => !string.IsNullOrEmpty(x.Remark));

            RuleFor(x => x.CouponCode)
                .MaximumLength(50).WithMessage("優惠碼最多 50 字元")
                .When(x => !string.IsNullOrEmpty(x.CouponCode));
        }
    }
}
