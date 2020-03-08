@echo off

set COMPILER=%cd%\Compiler\protoc
set SRC=.\Protocal\protoc
set DST=%cd%\Protocal\cs
set TPL=%cd%\Template
set ServerDst=E:\Work\GitHub\AuctionWar\GameServer\MainServer\AuctionWar

:: 客户端文件编译
echo LOG : begin compile client file.
del %DST%\*.cs /f /s /q /a
for %%i in (%SRC%\*.proto) do (
	echo compile file : %%~fi
	%COMPILER% %%i --csharp_out=%DST%
)

rem 编写 template
setlocal enabledelayedexpansion 
(for /f "delims=" %%i in (%TPL%\CSharpProtocTemplate.txt) do (
	set line=%%i
	
	if "!line!" NEQ "!line:#TypeBlock#=!" (
		for %%j in (%SRC%\*.proto) do (
			echo !line:#TypeBlock#=%%~nj,!
		)
	) else if "!line!" NEQ "!line:#ListenerBlock#=!" (
		for %%j in (%SRC%\*.proto) do (
			set name=%%~nj
			echo 	public sealed class !name!Listener : NetManager.BaseListener^<!name!Listener, !name!^>
			echo 	{
			echo 		public override short GetProtocType(^) =^> (Int16^)ProtocType.!name!;
			echo 		protected override !name! ParseData(byte[] data, int offset, int size^) =^> !name!.Parser.ParseFrom(data, offset, size^);
			echo 	}
		)
	) else (
		echo !line!
	)
))>>%DST%\Protoc.cs
setlocal disabledelayedexpansion

:: 服务端文件编译
echo LOG : begin compile server file.
del %ServerDst%\*.cs /f /s /q /a
for %%i in (%SRC%\*.proto) do (
	echo compile file : %%~fi
	%COMPILER% %%i --csharp_out=%ServerDst%
)

setlocal enabledelayedexpansion 
(for /f "delims=" %%i in (%TPL%\CSharpProtocTemplate.txt) do (
	set line=%%i
	
	if "!line!" NEQ "!line:#TypeBlock#=!" (
		for %%j in (%SRC%\*.proto) do (
			echo !line:#TypeBlock#=%%~nj,!
		)
	) else if "!line!" NEQ "!line:#ListenerBlock#=!" (
		for %%j in (%SRC%\*.proto) do (
			set name=%%~nj
			echo 	public sealed class !name!Listener : NetManager.BaseListener^<!name!Listener, !name!^>
			echo 	{
			echo 		public override short GetProtocType(^) =^> (Int16^)ProtocType.!name!;
			echo 		protected override !name! ParseData(byte[] data, int offset, int size^) =^> !name!.Parser.ParseFrom(data, offset, size^);
			echo 	}
		)
	) else (
		echo !line!
	)
))>>%ServerDst%\Protoc.cs
setlocal disabledelayedexpansion

pause