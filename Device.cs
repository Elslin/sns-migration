using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSEndPointCreation
{
    [Table("Device", Schema = "db_owner")]
    public class Device
    {
        [Key]
        public int id {  get; set; }
        public string appUserId {  get; set; }
        public int application { get; set; }
        public string token { get; set; }
        public char platform {  get; set; }
        public int numFailures {  get; set; }
        public DateTime? lastPush {  get; set; }
        public DateTime? lastFailure { get; set;}
        public string? targetPlatformEndPoint { get; set; }
        public string notificationSoundPref {  get; set; }
        public DateTime? createdOn { get; set; }
        public string? version {  get; set; }
        public string? deviceId {  get; set; }
    }
}
