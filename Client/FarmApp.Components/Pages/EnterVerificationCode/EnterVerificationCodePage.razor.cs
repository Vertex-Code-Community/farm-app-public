using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
namespace FarmApp.Components.Pages.EnterVerificationCode
{
    public partial class EnterVerificationCodePage
    {
        [Inject] private INavigationService NavigationService { get; set; } = null!;
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Inject] public required IAuthenticationService AuthenticationService { get; set; }

        private string[] otpValues = new string[4];
        private CancellationTokenSource? cts;
        private ElementReference[] inputRefs = new ElementReference[4];
        private string? ErrorMessage;
        private bool _isLoading = false;

        [Parameter] public string Email { get; set; } = string.Empty;
        [Parameter] public VerificationPurpose VerificationPurpose { get; set; }
        private void NavigateToNewPasswordCreate(string resetToken)
        {
            NavigationService.History.Clear();
            NavigationService.NavigateTo(Constants.ClientRoutes.TypePasswordPage, new Dictionary<string, object>
            {
                ["Email"] = Email,
                ["ResetToken"] = resetToken
            });
        }

        private async Task HandleInput(ChangeEventArgs e, int index)
        {
            var newValue = e.Value?.ToString();

            if (!string.IsNullOrEmpty(newValue) && newValue.Length > 1)
            {
                newValue = newValue.Substring(newValue.Length - 1);
            }

            otpValues[index] = newValue;

            if (!string.IsNullOrEmpty(newValue))
            {
                if (index < 3)
                {
                    await inputRefs[index + 1].FocusAsync();
                }
                else if (index == 3)
                {
                    await VerifyCode();
                }
            }
        }

        private async Task HandleBackspace(KeyboardEventArgs e, int index)
        {
            invalidCode = false;
            if (e.Key == "Backspace" && string.IsNullOrEmpty(otpValues[index]) && index > 0)
            {
                otpValues[index - 1] = "";
                await inputRefs[index - 1].FocusAsync();
            }
        }

        private bool invalidCode = false;

        private async Task VerifyCode()
        {
            string finalCode = string.Join("", otpValues);

            if (finalCode.Length == 4)
            {
                ErrorMessage = null;
                invalidCode = false;
                
                switch (VerificationPurpose)
                {

                    case VerificationPurpose.SignUp:
                    {
                        var result = await AuthenticationService.ConfirmEmailAsync(new ConfirmEmailRequestModel
                        {
                            Email = Email,
                            Code = finalCode
                        });
                        if (result.IsSuccess)
                        {
                            NavigationService.History.Clear();
                            NavigationService.NavigateTo(Constants.ClientRoutes.GuidePage);
                        }
                        HandleError(result.ErrorMessage);
                        break;
                    }
                    case VerificationPurpose.ResetPassword:
                    {
                        var result = await AuthenticationService.ValidateResetCode(new ResetCodeModel
                        {
                            Code = finalCode,
                            Email = Email,
                        });
                        if (result.IsSuccess)
                        {
                            NavigateToNewPasswordCreate(result.Data!.ResetToken);
                            return;
                        }
                        HandleError(result.ErrorMessage);
                        break;
                          
                    }
                }
            }
        }

        private int countdown = 59;
        private bool canResend = false;

        private void StartCountdown()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            _ = RunCountDownAsync(cts.Token);
        }
        private async Task RunCountDownAsync(CancellationToken token)
        {
            canResend = false;
            countdown = 59;

            try
            {
                while (countdown > 0 && !token.IsCancellationRequested)
                {
                    await Task.Delay(1000, token);
                    countdown--;
                    await InvokeAsync(StateHasChanged);
                }

                if (!token.IsCancellationRequested)
                {
                    canResend = true;
                    StateHasChanged();
                }
            }
            catch (TaskCanceledException)
            {
                // Countdown was cancelled, do nothing
            }
        }

        private async Task ResendCode()
        {
            if (!canResend) return;

            invalidCode = false;
            otpValues = new string[4];
            _ = inputRefs[0].FocusAsync();

            _isLoading = true;
            var result = await AuthenticationService.ResendCodeAsync(new FarmApp.ViewModels.Verifications.VerificationResendRequestModel
            {
                Email = Email,
                VerificationPurpose = VerificationPurpose
            });

            if (!result.IsSuccess)
            {
                HandleError(result.ErrorMessage);
            }

            _isLoading = false;
            StartCountdown();
        }

        protected override Task OnInitializedAsync()
        {
            StartCountdown();

            return Task.CompletedTask;
        }
        private void HandleError(string? error)
        {
            if (error == Constants.ErrorMessages.WRONG_CODE)
            {
                invalidCode = true;
                otpValues = new string[4];

                _ = inputRefs[0].FocusAsync();
            }
            ErrorMessage = Localizer[$"Error_{error}"];
        }
        public static string ObfuscateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];

            string maskedName;
            if (name.Length <= 2)
            {
                maskedName = name[0] + new string('*', 3);
            }
            else
            {
                maskedName = name.Substring(0, 2) + "****" + name.Substring(name.Length - 1);
            }

            var domainParts = domain.Split('.');
            string maskedDomain = domain;

            if (domainParts.Length >= 2)
            {
                var provider = domainParts[0];
                var extension = domainParts[1];
                maskedDomain = provider[0] + new string('*', provider.Length - 1) + "." + extension;
            }

            return $"{maskedName}@{maskedDomain}";
        }
    }
}

