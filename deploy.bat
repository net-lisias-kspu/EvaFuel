

set H=R:\KSP_1.2.2_dev
echo %H%

copy /Y "EVAFuel\bin\Debug\EVAFuel.dll" "GameData\EVAFuel\Plugins"
copy /Y "EvaFuel-KISCompat\bin\Debug\EvaFuelKISCompat.dll"  "GameData\EVAFuel\Plugins"
copy /Y EVAFuel.version GameData\EVAFuel

cd GameData
mkdir "%H%\GameData\EVAFuel"
xcopy /y /s EVAFuel "%H%\GameData\EVAFuel"
