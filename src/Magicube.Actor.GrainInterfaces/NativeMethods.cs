using System;
using System.Runtime.InteropServices;

namespace Magicube.Actor.GrainInterfaces {
    public class ConsoleUtility {
        public static void WriteLine(String fromat, ConsoleColor color, params object[] args) {
            Console.ForegroundColor = color;
            Console.WriteLine(fromat, args);
            Console.ResetColor();
        }
    }
    public class NativeMethods {
        public enum CtrlTypes {
            /// <summary>
            /// 当用户按下了CTRL+C,或者由GenerateConsoleCtrlEvent API发出. 
            /// </summary>
            CTRL_C_EVENT = 0,
            /// <summary>
            /// 用户按下CTRL+BREAK, 或者由GenerateConsoleCtrlEvent API发出.
            /// </summary>
            CTRL_BREAK_EVENT,
            /// <summary>
            /// 当试图关闭控制台程序，系统发送关闭消息。
            /// </summary>
            CTRL_CLOSE_EVENT,
            /// <summary>
            /// 用户退出时，但是不能决定是哪个用户. 
            /// </summary>
            CTRL_LOGOFF_EVENT = 5,
            /// <summary>
            /// 当系统被关闭时.  
            /// </summary>
            CTRL_SHUTDOWN_EVENT
        }
        public delegate bool HandlerRoutine(CtrlTypes ctrlType);
        public class WinNative {
            [DllImport("Kernel32")]
            public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);
        }
    }
}
