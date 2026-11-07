#pragma once

#include "common.h"

/* Returns "\\.\pipe\BuckShot_<UserSID>" (heap allocated, free with RtFree), or NULL */
LPWSTR GetBuckShotPipeName(void);

/*
 * Connects to the BuckShot pipe. If dwTimeoutMs > 0, retries until the pipe exists and is available.
 * dwFlagsAndAttributes is passed to CreateFileW (e.g. FILE_FLAG_OVERLAPPED).
 */
HANDLE ConnectToBuckShotPipe(LPCWSTR pwszPipeName, DWORD dwTimeoutMs, DWORD dwFlagsAndAttributes);

/*
 * Connects to the BuckShot pipe, starting BuckShot.exe (from the proxy's own directory) when it is not running.
 * A per-user mutex prevents several proxies from starting BuckShot at the same time.
 * BuckShot may then bring the window of the request to the front.
 */
HANDLE ConnectOrColdStart(LPCWSTR pwszPipeName);
