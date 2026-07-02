using FluentValidation;

namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 共享的 FluentValidation 擴充方法，重複利用「特定欄位」的驗證規則。
    /// 例如 Email、Password、Required Name 等在多個 DTO 中重複出現的欄位。
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>驗證 Email 格式且不可為空</summary>
        public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("電子郵件為必填欄位")
                .EmailAddress().WithMessage("電子郵件格式不正確")
                .MaximumLength(100).WithMessage("電子郵件最多 100 字元");
        }

        /// <summary>驗證密碼：不可為空，最少 6 碼</summary>
        public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("密碼為必填欄位")
                .MinimumLength(6).WithMessage("密碼至少 6 碼");
        }

        /// <summary>驗證必填欄位名稱（最長 200 字元）</summary>
        public static IRuleBuilderOptions<T, string> RequiredName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("名稱為必填欄位")
                .MaximumLength(200).WithMessage("名稱最多 200 字元");
        }

        /// <summary>驗證電話號碼：不可為空，7-20 碼數字</summary>
        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("電話號碼為必填欄位")
                .MinimumLength(7).WithMessage("電話號碼至少 7 碼")
                .MaximumLength(20).WithMessage("電話號碼最多 20 碼")
                .Matches(@"^[0-9+\-\s()]+$").WithMessage("電話號碼格式不正確");
        }

        /// <summary>驗證地址：不可為空</summary>
        public static IRuleBuilderOptions<T, string> RequiredAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("地址為必填欄位")
                .MaximumLength(500).WithMessage("地址最多 500 字元");
        }
    }
}
