@echo off
echo ========================================
echo     SwitchSound 音频设备切换工具
echo         完整构建脚本 v2.0
echo ========================================
echo.

echo 正在清理之前的构建文件...
dotnet clean > nul 2>&1
if exist "bin\Release" rmdir /s /q "bin\Release"

echo.
echo 构建版本1: 多文件依赖运行时版本
echo ----------------------------------------
dotnet publish -c Release -o "bin/Release/1-MultiFile-Runtime-Dependent"
if %ERRORLEVEL% NEQ 0 (
    echo 构建失败！
    pause
    exit /b 1
)

echo.
echo 构建版本2: 单文件依赖运行时版本 (推荐)
echo ----------------------------------------
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=false -p:IncludeNativeLibrariesForSelfExtract=true -o "bin/Release/2-SingleFile-Runtime-Dependent"
if %ERRORLEVEL% NEQ 0 (
    echo 构建失败！
    pause
    exit /b 1
)

echo.
echo 构建版本3: 单文件自包含版本
echo ----------------------------------------
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true -o "bin/Release/3-SingleFile-SelfContained"
if %ERRORLEVEL% NEQ 0 (
    echo 构建失败！
    pause
    exit /b 1
)

echo.
echo ========================================
echo           构建完成！
echo ========================================
echo.

for /f %%i in ('dir /b "bin\Release\1-MultiFile-Runtime-Dependent\*.exe"') do set "size1=%%~zi"
for /f %%i in ('dir /b "bin\Release\2-SingleFile-Runtime-Dependent\*.exe"') do set "size2=%%~zi"
for /f %%i in ('dir /b "bin\Release\3-SingleFile-SelfContained\*.exe"') do set "size3=%%~zi"

echo 版本1 (多文件依赖运行时): bin\Release\1-MultiFile-Runtime-Dependent\
echo   - 4个文件，主文件大小: %size1% 字节
echo   - 需要用户安装 .NET 6.0 运行时
echo   - 启动最快，内存占用最少
echo.
echo 版本2 (单文件依赖运行时): bin\Release\2-SingleFile-Runtime-Dependent\
echo   - 1个文件，大小: %size2% 字节 (推荐)
echo   - 需要用户安装 .NET 6.0 运行时
echo   - 便于分发，启动较快
echo.
echo 版本3 (单文件自包含): bin\Release\3-SingleFile-SelfContained\
echo   - 1个文件，大小: %size3% 字节
echo   - 无需安装任何运行时
echo   - 真正的绿色软件，可在任何Windows系统运行
echo.
echo 推荐使用版本2，如果用户没有.NET运行时则使用版本3
echo.
echo .NET 6.0 运行时下载: https://dotnet.microsoft.com/download/dotnet/6.0
echo.
pause 