namespace WebService.DTOs
{
    public class UsersLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}