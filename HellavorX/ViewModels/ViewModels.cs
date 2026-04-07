namespace HellavorX.ViewModels;

public class SelectedFile
{
    public string Name { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
}

public class CreatePostViewModel
{
    public string Content { get; set; } = string.Empty;
}

public class EditPostViewModel
{
    public string Content { get; set; } = string.Empty;
}

public class CreateCommentViewModel
{
    public string Content { get; set; } = string.Empty;
}

public class EditProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class SignupViewModel
{
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
