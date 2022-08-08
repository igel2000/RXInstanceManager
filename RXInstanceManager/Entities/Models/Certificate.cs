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

        public byte[] Body
        {
            get
            {
                return AppHelper.Base64Decode(BodyBase64);
            }
            set
            {
                BodyBase64 = AppHelper.Base64Encode(value);
            }
        }

        [Field("path", Size = 200)]
        public string Path { get; set; }
    }
}
