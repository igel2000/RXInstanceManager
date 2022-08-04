using System;
using SQLQueryGen;

namespace RXInstanceManager
{
    [Table("certificate")]
    public class Certificate
    {
        [Field("id", Key = true)]
        public int Id { get; set; }

        [Field("body")]
        public string BodyBase64 { get; set; }

        public string Body
        {
            get
            {
                return AppHelper.Base64DecodeToUTF8(BodyBase64);
            }
            set
            {
                BodyBase64 = AppHelper.Base64EncodeFromUTF8(value);
            }
        }

        [Field("path", Size = 200)]
        public string Path { get; set; }
    }
}
