using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaffeMaskineProjekt.DTO
{
    public class CreateUserModel
    {
        public required string Name { get; set; }
        public required string Password { get; set; }
    }
    public class EditUserModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }

    }
    public class LoginModel
    {
        public required string Name { get; set; }
        public required string Password { get; set; }
    }
}
