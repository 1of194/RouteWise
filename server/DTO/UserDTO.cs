namespace server.DTO
{
    public class UserDTO
    {
        public required string Username { get; set; }


        private string _password;

        public required string Password {
            get { return _password; } set { _password = value; }
        }
        public required PageType PageType { get; set; } = PageType.Register;


    }
    public enum PageType
    {

        Login = 1,

        Register = 2

    }
}
