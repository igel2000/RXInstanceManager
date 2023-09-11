using System;
using SQLQueryGen;

namespace RXInstanceManager
{
  [Table("config")]
  public class Config
  {
    [Field("id", Key = true)]
    public int Id { get; set; }

    [Field("instance")]
    [Navigate(TableName = "instance", FieldName = "id", Required = false)]
    public Instance Instance { get; set; }

    [Field("changed")]
    public DateTime Changed { get; set; }

    [Field("version", Size = 20)]
    public string Version { get; set; }

    [Field("path", Size = 200)]
    public string Path { get; set; }

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
  }
}
