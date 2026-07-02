using FluentValidation;

namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 驗證登入 DTO：Email 格式 + 密碼不可為空。
    /// </summary>
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .ValidEmail();

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("密碼為必填欄位");
        }
    }

    /// <summary>
    /// 驗證使用者註冊 DTO。
    /// </summary>
    public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
    {
        public UserRegisterDtoValidator()
        {
            RuleFor(x => x.username)
                .NotEmpty().WithMessage("使用者名稱為必填欄位")
                .MaximumLength(50).WithMessage("使用者名稱最多 50 字元");

            RuleFor(x => x.email)
                .ValidEmail();

            RuleFor(x => x.password)
                .Password();
        }
    }

    /// <summary>
    /// 驗證修改密碼 DTO：目前密碼 + 新密碼。
    /// </summary>
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("目前密碼為必填欄位");

            RuleFor(x => x.NewPassword)
                .Password();
        }
    }

    /// <summary>
    /// 驗證 Email 驗證碼 DTO。
    /// </summary>
    public class VerifyCodeDtoValidator : AbstractValidator<VerifyCodeDto>
    {
        public VerifyCodeDtoValidator()
        {
            RuleFor(x => x.Email)
                .ValidEmail();

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("驗證碼為必填欄位")
                .Length(6).WithMessage("驗證碼必須為 6 碼");
        }
    }
}
