using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.Domain.EPay
{
    public class ePayAPIs
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string ApiName { get; set; }

        public string RequestURL { get; set; }

        [StringLength(1000)]
        public string AccessToken { get; set; }

        [StringLength(100)]
        public string ClientId { get; set; }

        [StringLength(500)]
        public string ClientSecretKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? TokenExpiredOn { get; set; }
    }
}
