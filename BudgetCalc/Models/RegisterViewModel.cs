using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    public string UserName { get; set; }  // Dodane pole loginu

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
    public string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "Hasła się nie zgadzają.")]
    public string ConfirmPassword { get; set; }
}
