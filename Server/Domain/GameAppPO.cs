using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("game_app")]
    public class GameAppPO : BaseEntity<long?, string?>
    {
        [Column("game_id")]
        public long? GameId { get; set; }
        /// 平台
        [Column("platform")]
        public string? Platform { get; set; }
        /// 应用id
        [Column("app_id")]
        public string? AppId { get; set; }
        /// 应用密钥
        [Column("app_secret")]
        public string? AppSecret { get; set; }
        /// 推送密钥
        [Column("app_secret_push")]
        public string? AppSecretPush { get; set; }

        public static GameAppPO Empty()
        {
            return new ()
            {
                Id = default(long),
                GameId = default(long),
                Platform = string.Empty,
                AppId = string.Empty,
                AppSecret = string.Empty,
                AppSecretPush = string.Empty,
            };
        }
    }
}
