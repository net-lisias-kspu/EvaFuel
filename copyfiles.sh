#!/bin/bash
RELEASEDIR=../../EvaFuel
rm -rf $RELEASEDIR
mkdir -p "$RELEASEDIR/Plugins/"
cp KIS_Fuel_Tank.cfg "$RELEASEDIR"
cp LICENSE "$RELEASEDIR"
cp CREDITS "$RELEASEDIR"
cp EvaFuel.version "$RELEASEDIR"
cp ./EvaFuel/bin/Release/EvaFuel.dll "$RELEASEDIR/Plugins/"
cp ./EvaFuel-KISCompat/bin/Release/EvaFuel-KISCompat.dll "$RELEASEDIR/Plugins/"
