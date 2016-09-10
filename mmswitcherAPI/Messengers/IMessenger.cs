using System;
namespace mmswitcherAPI.Messengers
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// 
        /// </summary>
        string Caption { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// 
        /// </summary>
        Messenger Messenger { get; }

        /// <summary>
        /// 
        /// </summary>
        bool Focused { get; }

        int NewMessagesCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        event newMessageDelegate GotNewMessage;

        /// <summary>
        /// 
        /// </summary>
        event newMessageDelegate MessageGone;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler GotFocus;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler LostFocus;

    }
}
