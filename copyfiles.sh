#!/bin/bash
RELEASEDIR=../../EvaFuel/
rm "$RELEASEDIR/*"
cp KIS_Fuel_Tank.cfg "$RELEASEDIR"
cp LICENSE "$RELEASEDIR"
cp CREDITS "$RELEASEDIR"
cp ./EvaFuel/bin/Release/EvaFuel.dll "$RELEASEDIR"
cp ./EvaFuel-KISCompat/bin/Release/EvaFuel-KISCompat.dll "$RELEASEDIR"