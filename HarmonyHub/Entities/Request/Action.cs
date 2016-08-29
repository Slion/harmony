using System.Runtime.Serialization;

namespace HarmonyHub
{
    /// <summary>
    ///     HarmonyHub Remote Action
    /// </summary>
    [DataContract]
    public class Action
    {
        /// <summary>
        ///     HarmonyHub command to send to a device.
        ///     This is the command string expected as input to our SendCommand and SendKeyPress APIs.
        /// </summary>
        [DataMember(Name = "command")]
        public string Command { get; set; }

        /// <summary>
        ///     DeviceId to receive command
        /// </summary>
        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        ///     Action Type (IRCommand)
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}