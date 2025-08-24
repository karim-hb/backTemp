using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Narije.Core.DTOs.Login
{
    /// <summary>
    /// ویو مدل لاگین کاربر
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// نام کاربری
        /// </summary>
        public string mobile { get; set; }

        /// <summary>
        /// گذرواژه
        /// </summary>

        public string password { get; set; }

        public string panel { get; set; }
    }
}
