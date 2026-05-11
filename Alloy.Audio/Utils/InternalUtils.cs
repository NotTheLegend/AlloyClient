using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;
using StringName = OpenTK.Audio.OpenAL.ALC.StringName;

namespace Alloy.Audio.Utils;

internal static class InternalUtils {

    internal static string GetAudioBinaryPath() {
        var is64 = Environment.Is64BitProcess;

        string platform;
        string file;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) {
            platform = "linux-x64";
            file = "libopenal.so";
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            platform = is64 ? "win-x64" : "win-x86";
            file = "soft_oal.dll";
        } else {
            throw new NotSupportedException($"The library name couldn't be resolved for the given platform ('{RuntimeInformation.OSDescription}').");
        }

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"runtimes\{platform}\native\{file}");
    }

    internal static Format GetChannelFormat(int channels) {
        return channels switch {
            1 => Format.FormatMono16,
            2 => Format.FormatStereo16,
            _ => throw new ArgumentOutOfRangeException(nameof(channels), channels, "Not mono or stereo")
        };
    }

    extension(ALC) {
        internal static string GetDefaultDevice() {
            return ALC.GetString(ALCDevice.Null, StringName.DefaultAllDevicesSpecifier);
        }

        internal static unsafe string[] GetAllDevices() {
            var devices = new List<string>();
            var position = ALC.GetString_(ALCDevice.Null, StringName.AllDevicesSpecifier);

            while (true) {
                var currentString = Marshal.PtrToStringAnsi(new IntPtr(position));
                if (string.IsNullOrEmpty(currentString)) {
                    break;
                }

                devices.Add(currentString);
                position += Encoding.UTF8.GetByteCount(currentString) + 1;
            }

            return devices.ToArray();
        }
    }
    
    extension(AL) {
        internal static void SourceQueueBuffers(int source, int count, ReadOnlySpan<int> buffers) {
            AL.SourceQueueBuffers(source, count, MemoryMarshal.Cast<int, uint>(buffers));
        }

        internal static void SourceQueueBuffer(int source, int buffer) {
            AL.SourceQueueBuffers(source, 1, MemoryMarshal.Cast<int, uint>(MemoryMarshal.CreateReadOnlySpan(ref buffer, 1)));
        }
    }
}