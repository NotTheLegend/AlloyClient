using System;
using System.Runtime.InteropServices;

namespace AlloyClient.Editor;

internal static class NativeFileDialog {
    private const int Explorer = 0x00080000;
    private const int FileMustExist = 0x00001000;
    private const int PathMustExist = 0x00000800;
    private const int OverwritePrompt = 0x00000002;
    private const int NoChangeDirectory = 0x00000008;
    private const int FileBufferCharacters = 4096;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OpenFileName {
        public int StructSize;
        public IntPtr Owner;
        public IntPtr Instance;
        [MarshalAs(UnmanagedType.LPWStr)] public string Filter;
        public IntPtr CustomFilter;
        public int MaxCustomFilter;
        public int FilterIndex;
        public IntPtr File;
        public int MaxFile;
        public IntPtr FileTitle;
        public int MaxFileTitle;
        public IntPtr InitialDirectory;
        [MarshalAs(UnmanagedType.LPWStr)] public string Title;
        public int Flags;
        public short FileOffset;
        public short FileExtension;
        [MarshalAs(UnmanagedType.LPWStr)] public string DefaultExtension;
        public IntPtr CustomData;
        public IntPtr Hook;
        public IntPtr TemplateName;
        public IntPtr Reserved;
        public int Reserved2;
        public int FlagsEx;
    }

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetOpenFileNameW(ref OpenFileName data);

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSaveFileNameW(ref OpenFileName data);

    public static string OpenMap() {
        if (!OperatingSystem.IsWindows()) return null;

        var fileBuffer = AllocateFileBuffer(null);
        try {
            var data = Create("Open map", Explorer | FileMustExist | PathMustExist, fileBuffer);
            return GetOpenFileNameW(ref data) ? Marshal.PtrToStringUni(fileBuffer) : null;
        } finally {
            Marshal.FreeHGlobal(fileBuffer);
        }
    }

    public static string SaveMap(string suggestedName, bool wmap) {
        if (!OperatingSystem.IsWindows()) return null;

        var extension = wmap ? "wmap" : "jm";
        var fileName = suggestedName.EndsWith($".{extension}", StringComparison.OrdinalIgnoreCase)
            ? suggestedName
            : $"{suggestedName}.{extension}";

        var fileBuffer = AllocateFileBuffer(fileName);

        try {
            var data = Create("Save map", Explorer | PathMustExist | OverwritePrompt, fileBuffer);
            data.DefaultExtension = extension;
            data.FilterIndex = wmap ? 2 : 1;
            return GetSaveFileNameW(ref data) ? Marshal.PtrToStringUni(fileBuffer) : null;
        } finally {
            Marshal.FreeHGlobal(fileBuffer);
        }
    }

    private static OpenFileName Create(string title, int flags, IntPtr fileBuffer) {
        return new OpenFileName {
            StructSize = Marshal.SizeOf<OpenFileName>(),
            Title = title,
            Flags = flags | NoChangeDirectory,
            Filter = "JSON Map (*.jm)\0*.jm\0World Map (*.wmap)\0*.wmap\0All Map Files\0*.jm;*.wmap\0\0",
            FilterIndex = 1,
            File = fileBuffer,
            MaxFile = FileBufferCharacters,
        };
    }

    private static IntPtr AllocateFileBuffer(string initialValue) {
        var buffer = Marshal.AllocHGlobal(FileBufferCharacters * sizeof(char));
        for (var offset = 0; offset < FileBufferCharacters * sizeof(char); offset += sizeof(char))
            Marshal.WriteInt16(buffer, offset, 0);

        if (string.IsNullOrEmpty(initialValue)) return buffer;

        var characters = initialValue.ToCharArray();
        var count = Math.Min(characters.Length, FileBufferCharacters - 1);
        Marshal.Copy(characters, 0, buffer, count);
        return buffer;
    }
}