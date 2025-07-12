@echo off
echo 正在构建 SwitchSound...

REM 清理输出目录
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj" rmdir /s /q "obj"

REM 构建应用程序
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo 构建成功！
    echo 输出文件位置: bin\Release\net6.0-windows\win-x64\publish\SwitchSound.exe
    echo.
    pause
) else (
    echo.
    echo 构建失败！
    echo.
    pause
) 