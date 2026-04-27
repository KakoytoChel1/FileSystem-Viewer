using System.Runtime.InteropServices;

namespace FileSystemViewer.ContextMenu
{
    [ComVisible(true)]
    [Guid("E7200463-6915-4F27-ADFA-F5BD16F9D876")]
    public class FolderContextMenuCommand : IExplorerCommand
    {
        // Системные коды ответов (HRESULT)
        private const int S_OK = 0;
        private const int E_NOTIMPL = unchecked((int)0x80004001);

        public int GetTitle(IShellItemArray psiItemArray, out string ppszName)
        {
            ppszName = "Open in FileSystemViewer";
            return S_OK; // Успех
        }

        public int GetIcon(IShellItemArray psiItemArray, out string ppszIcon)
        {
            ppszIcon = null!; // Обязательно null, а не пустая строка
            return E_NOTIMPL; // Говорим проводнику: "Иконки нет, не ищи"
        }

        public int GetToolTip(IShellItemArray psiItemArray, out string ppszInfotip)
        {
            ppszInfotip = "Open the selected folder in FileSystemViewer";
            return S_OK;
        }

        public int GetCanonicalName(out Guid pguidCommandName)
        {
            pguidCommandName = new Guid("E7200463-6915-4F27-ADFA-F5BD16F9D876");
            return S_OK;
        }

        public int GetState(IShellItemArray psiItemArray, bool fOkToBeSlow, out uint pcsFlags)
        {
            pcsFlags = 0; // ECS_ENABLED
            return S_OK;
        }

        public int GetFlags(out uint pFlags)
        {
            pFlags = 0;
            return S_OK;
        }

        public int EnumSubCommands(out IntPtr ppEnum)
        {
            ppEnum = IntPtr.Zero;
            return E_NOTIMPL;
        }

        public int Invoke(IShellItemArray psiItemArray, IntPtr pbc)
        {
            try
            {
                psiItemArray.GetCount(out uint count);
                if (count > 0)
                {
                    psiItemArray.GetItemAt(0, out IShellItem item);
                    item.GetDisplayName(unchecked((int)0x80058000), out IntPtr namePtr); // SIGDN_FILESYSPATH

                    if (namePtr != IntPtr.Zero)
                    {
                        string folderPath = Marshal.PtrToStringUni(namePtr);
                        Marshal.FreeCoTaskMem(namePtr);

                        // 1. Узнаем, в какой папке лежит наша запущенная DLL
                        string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        string appFolder = System.IO.Path.GetDirectoryName(dllPath);

                        // 2. Склеиваем путь к вашему основному EXE
                        string exePath = System.IO.Path.Combine(appFolder, "FileSystemViewer.exe");

                        // 3. Запускаем основное окно приложения
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = $"-OpenFolder \"{folderPath}\"",
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch
            {
                // Silently ignore errors in COM context
            }

            return S_OK;
        }
    }
}
