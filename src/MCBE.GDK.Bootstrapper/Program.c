#define _MINAPPMODEL_H_
#include <windows.h>
#include <appmodel.h>

VOID WinMainCRTStartup()
{
    WCHAR pfn1[PACKAGE_FULL_NAME_MAX_LENGTH + 1] = {};
    if (GetCurrentPackageFullName(&(UINT){ARRAYSIZE(pfn1)}, pfn1))
        ExitProcess(EXIT_FAILURE);

    WCHAR path[MAX_PATH] = {};
    if (GetPackagePathByFullName(pfn1, &(UINT){ARRAYSIZE(path)}, path))
        ExitProcess(EXIT_FAILURE);

    if (!SetCurrentDirectoryW(path))
        ExitProcess(EXIT_FAILURE);

    HANDLE mutex = CreateMutexW(NULL, FALSE, pfn1);

    if (!mutex)
        ExitProcess(EXIT_FAILURE);

    if (mutex && !GetLastError())
    {
        PROCESS_INFORMATION info = {};
        CreateProcessW(L"Minecraft.Windows.exe", NULL, NULL, NULL, FALSE, 0, NULL, NULL, &(STARTUPINFOW){}, &info);

        HANDLE target = NULL;
        DuplicateHandle(GetCurrentProcess(), mutex, info.hProcess, &target, DUPLICATE_SAME_ACCESS, FALSE,
                        DUPLICATE_CLOSE_SOURCE);
        CloseHandle(target);

        CloseHandle(info.hThread);
        CloseHandle(info.hProcess);
    }
    else
    {
        HWND wnd = {};
        WCHAR pfn2[PACKAGE_FULL_NAME_MAX_LENGTH + 1] = {};

        while ((wnd = FindWindowExW(NULL, wnd, L"Bedrock", NULL)))
        {
            DWORD id = 0;
            GetWindowThreadProcessId(wnd, &id);

            HANDLE process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, id);

            if (GetPackageFullName(process, &(UINT){ARRAYSIZE(pfn2)}, pfn2) ||
                CompareStringOrdinal(pfn1, -1, pfn2, -1, TRUE) != CSTR_EQUAL)
            {
                CloseHandle(process);
                continue;
            }

            SwitchToThisWindow(wnd, TRUE);
            CloseHandle(process);
            break;
        }
    }

    CloseHandle(mutex);
    ExitProcess(EXIT_SUCCESS);
}