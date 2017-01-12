#!/bin/bash
RELEASEDIR=../Release/EvaFuel
rm -rf $RELEASEDIR
mkdir -p "$RELEASEDIR/Plugins/"
cp KIS_Fuel_Tank.cfg "$RELEASEDIR"
cp EvaFuelSettings.cfg "$RELEASEDIR"
cp LICENSE "$RELEASEDIR"
cp CREDITS "$RELEASEDIR"
cp EvaFuel.version "$RELEASEDIR"
cp ./EvaFuel/bin/Release/EvaFuel.dll "$RELEASEDIR/Plugins/"
cp ./EvaFuel-KISCompat/bin/Release/EvaFuel-KISCompat.dll "$RELEASEDIR/Plugins/"
cp ../Mod\ Assemblies/KSPDev_Utils.dll "$RELEASEDIR/Plugins/"
cd ../Release/
zip -r -9 "EvaFuel-$(date --universal +%F-%T).zip" "EvaFuel"
