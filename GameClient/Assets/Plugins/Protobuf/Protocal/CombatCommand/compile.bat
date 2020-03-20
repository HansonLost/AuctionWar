@echo off

set COMPILER=E:\Work\GitHub\AuctionWar\GameClient\Assets\Plugins\Protobuf\Compiler\protoc
set SRC=E:\Work\GitHub\AuctionWar\GameClient\Assets\Plugins\Protobuf\Protocal\CombatCommand\proto
set DST=E:\Work\GitHub\AuctionWar\GameClient\Assets\Plugins\Protobuf\Protocal\CombatCommand\cs

:: 对战命令参数编译
echo begin complie......
del %DST%\*.cs /f /s /q /a
for %%i in (%SRC%\*.proto) do (
	echo compile file : %%~fi
	%COMPILER% %%i -I=%SRC% --csharp_out=%DST%
)

pause